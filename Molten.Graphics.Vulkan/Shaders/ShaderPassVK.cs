using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal unsafe class ShaderPassVK : HlslPass
    {
        /// <summary>
        /// A list of all shader stages in the order they are expected by Vulkan and Molten.
        /// </summary>
        internal static readonly ShaderType[] ShaderTypes = new ShaderType[] {
            ShaderType.Vertex,
            ShaderType.Hull,
            ShaderType.Domain,
            ShaderType.Geometry,
            ShaderType.Pixel,
            ShaderType.Compute
        };

        internal static readonly Dictionary<ShaderType, ShaderStageFlags> ShaderStageLookup = new Dictionary<ShaderType, ShaderStageFlags>()
        {
            [ShaderType.Vertex] = ShaderStageFlags.VertexBit,
            [ShaderType.Hull] = ShaderStageFlags.TessellationControlBit,
            [ShaderType.Domain] = ShaderStageFlags.TessellationEvaluationBit,
            [ShaderType.Geometry] = ShaderStageFlags.GeometryBit,
            [ShaderType.Pixel] = ShaderStageFlags.FragmentBit
        };

        GraphicsPipelineCreateInfo _info;
        Pipeline _pipeline;

        BlendStateVK _blendState;
        DepthStateVK _depthState;
        RasterizerStateVK _rasterizerState;
        DynamicStateVK _dynamicState;
        InputAssemblyStateVK _inputState;

        internal ShaderPassVK(HlslShader material, string name = null) : 
            base(material, name)
        {
            _info = new GraphicsPipelineCreateInfo();
            _info.SType = StructureType.GraphicsPipelineCreateInfo;
            _info.Flags = PipelineCreateFlags.None;
            _info.PNext = null;
        }

        protected override void OnInitialize(ref ShaderPassParameters parameters)
        {
            // Populate dynamic state
            DeviceVK device = Device as DeviceVK;
            _blendState = new BlendStateVK(device, ref parameters);
            _blendState = device.CacheObject(_blendState.Desc, _blendState);

            _depthState = new DepthStateVK(device, ref parameters);
            _depthState = device.CacheObject(_depthState.Desc, _depthState);

            _rasterizerState = new RasterizerStateVK(device, ref parameters);
            _rasterizerState = device.CacheObject(_rasterizerState.Desc, _rasterizerState);

            _inputState = new InputAssemblyStateVK(device, ref parameters);
            _inputState = device.CacheObject(_inputState.Desc, _inputState);

            DynamicState[] dynamics = new DynamicState[]
            {
                DynamicState.ViewportWithCount,
                DynamicState.ScissorWithCount,
            };

            _dynamicState = new DynamicStateVK(device, ref parameters, dynamics);
            _dynamicState = device.CacheObject(_dynamicState.Desc, _dynamicState);

            // Setup shader stage info
            _info.PStages = EngineUtil.AllocArray<PipelineShaderStageCreateInfo>((uint)CompositionCount);
            _info.StageCount = 0;

            // Iterate over and add pass compositions in the order Vulkan expects.
            foreach (ShaderType type in ShaderTypes)
            {
                ShaderComposition c = this[type];
                if (c == null)
                    continue;

                ref PipelineShaderStageCreateInfo stageDesc = ref _info.PStages[_info.StageCount++];
                stageDesc.SType = StructureType.PipelineShaderStageCreateInfo;
                stageDesc.PName = (byte*)SilkMarshal.StringToPtr(c.EntryPoint, NativeStringEncoding.UTF8);
                stageDesc.Stage = ShaderStageLookup[type];
                stageDesc.Module = *(ShaderModule*)c.PtrShader;
                stageDesc.Flags = PipelineShaderStageCreateFlags.None;
                stageDesc.PNext = null;
            }

            _info.PMultisampleState = null;                         // TODO initialize
            _info.Layout = new PipelineLayout();                    // TODO initialize - DescriptorLayoutVK goes in here
            _info.BasePipelineIndex = 0;                            // TODO initialize
            _info.BasePipelineHandle = new Pipeline();              // TODO initialize or use derivative piplines. Implement pipeline inheritance.
            _info.PTessellationState = null;                        // TODO initialize
            _info.PVertexInputState = null;                         // TODO initialize
            _info.PViewportState = null;                            // Ignored. Set in dynamic state.
            _info.RenderPass = new RenderPass();                    // TODO initialize
            _info.Subpass = 0;                                      // TODO initialize

            _info.PColorBlendState = _blendState.Desc;
            _info.PRasterizationState = _rasterizerState.Desc;
            _info.PDepthStencilState = _depthState.Desc;
            _info.PDynamicState = _dynamicState.Desc;
            _info.PInputAssemblyState = _inputState.Desc;

            // Create pipeline.
            fixed(GraphicsPipelineCreateInfo* pResult = &_info)
            {
                fixed(Pipeline* ptrPipeline = &_pipeline)
                    device.VK.CreateGraphicsPipelines(device, new PipelineCache(), 1, _info, null, ptrPipeline);
            }
        }

        protected override void OnGraphicsRelease()
        {
            DeviceVK device = Device as DeviceVK;

            // Release indirect memory allocations for pipleine shader stages
            for (uint i = 0; i < _info.StageCount; i++)
            {
                ref PipelineShaderStageCreateInfo stageDesc = ref _info.PStages[i];
                SilkMarshal.FreeString((nint)stageDesc.PName);
            }

            EngineUtil.Free(ref _info.PStages);

            if(_pipeline.Handle != 0)
            {             
                device.VK.DestroyPipeline(device, _pipeline, null);
                _pipeline = new Pipeline();
            }
        }

        public static implicit operator Pipeline(ShaderPassVK pass)
        {
            return pass._pipeline;
        }
    }
}
