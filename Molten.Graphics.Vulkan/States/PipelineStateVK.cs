using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal unsafe class PipelineStateVK : GraphicsObject
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
        List<PipelineStateVK> _derivatives = new List<PipelineStateVK>();

        BlendStateVK _blendState;
        DepthStateVK _depthState;
        RasterizerStateVK _rasterizerState;
        DynamicStateVK _dynamicState;
        InputAssemblyStateVK _inputState;
        DescriptorSetLayoutVK _descriptorLayout;
        PipelineLayoutVK _pipelineLayout;

        AttachmentDescription[] _attachments; // Last attachment(s) is always depth/stencil.

        public PipelineStateVK(DeviceVK device, ShaderPassVK pass, ref ShaderPassParameters parameters) : 
            base(device)
        {
            _attachments = new AttachmentDescription[0];

            _info = new GraphicsPipelineCreateInfo();
            _info.SType = StructureType.GraphicsPipelineCreateInfo;
            _info.Flags = PipelineCreateFlags.None;
            _info.PNext = null;

            // Populate dynamic state
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
            _info.PStages = EngineUtil.AllocArray<PipelineShaderStageCreateInfo>((uint)pass.CompositionCount);
            _info.StageCount = 0;

            // Iterate over and add pass compositions in the order Vulkan expects.
            foreach (ShaderType type in ShaderTypes)
            {
                ShaderComposition c = pass[type];
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

            _descriptorLayout = new DescriptorSetLayoutVK(device, pass);
            _pipelineLayout = new PipelineLayoutVK(device, _descriptorLayout);

            _info.PMultisampleState = null;                         // TODO initialize
            _info.BasePipelineIndex = 0;                            // TODO initialize
            _info.BasePipelineHandle = new Pipeline();              // TODO initialize 
            _info.PTessellationState = null;                        // TODO initialize
            _info.PVertexInputState = null;                         // TODO initialize
            _info.PViewportState = null;                            // Ignored. Set in dynamic state.
            _info.RenderPass = new RenderPass();                    // TODO initialize - This should be stored in RenderStep based on inputput and output surfaces.
            _info.Subpass = 0;                                      // TODO initialize

            _info.PColorBlendState = _blendState.Desc;
            _info.PRasterizationState = _rasterizerState.Desc;
            _info.PDepthStencilState = _depthState.Desc;
            _info.PDynamicState = _dynamicState.Desc;
            _info.PInputAssemblyState = _inputState.Desc;
            _info.Layout = _pipelineLayout.Handle;

            /* TODO Implement derivative pipeline support:
             *   - ShaderPass will provide a base PassPipelineVK instance
             *   - GraphicsDevice will store PassPipelineVK instances by ShaderPassVK and render surface count + type
             *   - GraphicsQueue.ApplyRenderState() or ApplyComputeState() must check the pipeline cache for a matching pipelines based on: 
             *      - Bound render surfaces
             *   - See: https://registry.khronos.org/vulkan/specs/1.3-extensions/html/vkspec.html#pipelines-pipeline-derivatives
             *      - Ensure VK_PIPELINE_CREATE_DERIVATIVE_BIT is set on derivative pipelines.
             *      - Base pipeline must set VK_PIPELINE_CREATE_ALLOW_DERIVATIVES_BIT 
             *      
             *   - Implement PipelineCacheVK:
             *      - Implement PipelineVK and move all properties out of ShaderPassVK and into PipelineVK
             */

            // Create pipeline.
            fixed (GraphicsPipelineCreateInfo* pResult = &_info)
            {
                fixed (Pipeline* ptrPipeline = &_pipeline)
                    device.VK.CreateGraphicsPipelines(device, new PipelineCache(), 1, _info, null, ptrPipeline);
            }

            // TODO after render/compute pass, reset the load-op of surfaces.
        }

        private PipelineStateVK(DeviceVK device, PipelineStateVK baseState, IRenderSurfaceVK[] surfaces, DepthSurfaceVK depthSurface) :
            base(device)
        {
            if (baseState == null)
                throw new ArgumentNullException(nameof(baseState), "Base state cannot be null");

            int attachmentCount = surfaces.Length + (depthSurface != null ? 1 : 0);
            _attachments = new AttachmentDescription[attachmentCount];
            _info = new GraphicsPipelineCreateInfo();
            _info.SType = StructureType.GraphicsPipelineCreateInfo;
            _info.Flags = PipelineCreateFlags.None;
            _info.PNext = null;
            _info.BasePipelineHandle = baseState;
            BaseState = baseState;

            for (int i = 0; i < surfaces.Length; i++)
                GetAttachmentDesc(surfaces[i], out _attachments[i]);
        }

        internal PipelineStateVK GetState(DepthSurfaceVK depth, params IRenderSurfaceVK[] surfaces)
        {
            DeviceVK device = Device as DeviceVK;

            if (DoSurfacesMatch(device, surfaces, depth))
                return this;
            else
                return GetDerivation(device, surfaces, depth);
        }

        private PipelineStateVK GetDerivation(DeviceVK device, IRenderSurfaceVK[] surfaces, DepthSurfaceVK depthSurface)
        {
            // Check if we have an existing derivative that matches our surface attachments.
            foreach (PipelineStateVK derivative in _derivatives)
            {
                if(derivative.DoSurfacesMatch(device, surfaces, depthSurface))
                    return derivative;  
            }

            PipelineStateVK derivation = new PipelineStateVK(device, this, surfaces, depthSurface);
            _derivatives.Add(derivation);
            return derivation;
        }

        private bool DoSurfacesMatch(DeviceVK device, IRenderSurfaceVK[] surfaces, DepthSurfaceVK depthSurface)
        {
            AttachmentDescription descCompare = new AttachmentDescription();

            int surfaceCount = surfaces.Length + (depthSurface != null ? 1 : 0);
            if (surfaceCount == _attachments.Length)
            {
                int i = 0;
                for (; i < surfaces.Length; i++)
                {
                    GetAttachmentDesc(surfaces[i], out descCompare);
                    if (!AttachmentsEqual(ref _attachments[i], ref descCompare))
                        return false;
                }

                // Compare the last attachment to the depth surface, if present.
                if(depthSurface != null)
                {
                    GetAttachmentDesc(depthSurface, out descCompare);
                    if (!AttachmentsEqual(ref _attachments[i], ref descCompare))
                        return false;
                }
            }

            return true;
        }

        private void GetAttachmentDesc(IRenderSurfaceVK surface, out AttachmentDescription desc)
        {
            desc = new AttachmentDescription()
            {
                Format = surface.ResourceFormat.ToApi(),
                Samples = GetSampleFlags(surface.MultiSampleLevel),
                LoadOp = surface.ClearColor.HasValue ? AttachmentLoadOp.Clear : AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store,
                InitialLayout = ImageLayout.Undefined, // TODO Track current layout of texture/surface and use it here
                FinalLayout = ImageLayout.Undefined,
                StencilLoadOp = AttachmentLoadOp.DontCare,
                StencilStoreOp = AttachmentStoreOp.DontCare,
            };

            if (surface is ISwapChainSurface)
            {
                desc.FinalLayout = ImageLayout.PresentSrcKhr;
            }
            else
            {
                if (!surface.Flags.Has(GraphicsResourceFlags.NoShaderAccess))
                    desc.FinalLayout = ImageLayout.ColorAttachmentOptimal;
                else
                    desc.FinalLayout = ImageLayout.ShaderReadOnlyOptimal;
            }
        }

        private void GetAttachmentDesc(DepthSurfaceVK surface, out AttachmentDescription desc)
        {
            desc = new AttachmentDescription()
            {
                Format = surface.ResourceFormat.ToApi(),
                Samples = GetSampleFlags(surface.MultiSampleLevel),
                LoadOp = surface.ClearValue.HasValue ? AttachmentLoadOp.Clear : AttachmentLoadOp.Load,
                StoreOp = AttachmentStoreOp.Store,
                StencilLoadOp = AttachmentLoadOp.DontCare, // TODO Set these based on depthSurface.DepthFormat - stencil format
                StencilStoreOp = AttachmentStoreOp.DontCare,
                InitialLayout = ImageLayout.Undefined,
                FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
                // TODO Make use of DepthStencilReadOnlyOptimal when using read-only depth mode.
                // TODO if we don't need a stencil, try attaching with DepthAttachmentOptimal instead.
            };
        }


        internal AttachmentReference GetAttachmentRef(ref AttachmentDescription desc, uint index)
        {
            /* For depth surface,s if the separateDepthStencilLayouts feature is not enabled, and attachment is not VK_ATTACHMENT_UNUSED, layout must not be:
             *  - VK_IMAGE_LAYOUT_DEPTH_ATTACHMENT_OPTIMAL, VK_IMAGE_LAYOUT_DEPTH_READ_ONLY_OPTIMAL, VK_IMAGE_LAYOUT_STENCIL_ATTACHMENT_OPTIMAL or 
             *  VK_IMAGE_LAYOUT_STENCIL_READ_ONLY_OPTIMAL*/

            ImageLayout layout = ImageLayout.ColorAttachmentOptimal;
            if (desc.FinalLayout == ImageLayout.DepthStencilAttachmentOptimal
                || desc.FinalLayout == ImageLayout.DepthStencilReadOnlyOptimal
                || desc.FinalLayout == ImageLayout.DepthAttachmentOptimal
                || desc.FinalLayout == ImageLayout.DepthReadOnlyOptimal)
            {
                layout = ImageLayout.DepthStencilAttachmentOptimal;
            }

            return new AttachmentReference(index, layout);
        }

        private SampleCountFlags GetSampleFlags(AntiAliasLevel aaLevel)
        {
            switch (aaLevel)
            {
                default:
                case AntiAliasLevel.None:
                    return SampleCountFlags.Count1Bit;

                case AntiAliasLevel.X2:
                    return SampleCountFlags.Count2Bit;

                case AntiAliasLevel.X4:
                    return SampleCountFlags.Count4Bit;

                case AntiAliasLevel.X8:
                    return SampleCountFlags.Count8Bit;

                case AntiAliasLevel.X16:
                    return SampleCountFlags.Count16Bit;
            }
        }

        private bool AttachmentsEqual(ref AttachmentDescription a, ref AttachmentDescription b)
        {
            return a.FinalLayout == b.FinalLayout &&
                a.Format == b.Format &&
                a.InitialLayout == b.InitialLayout &&
                a.LoadOp == b.LoadOp &&
                a.Samples == b.Samples &&
                a.StencilLoadOp == b.StencilLoadOp &&
                a.StencilStoreOp == b.StencilStoreOp &&
                a.StoreOp == b.StoreOp;
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

            _pipelineLayout.Dispose();
            _descriptorLayout.Dispose();

            if (_pipeline.Handle != 0)
            {
                device.VK.DestroyPipeline(device, _pipeline, null);
                _pipeline = new Pipeline();
            }
        }

        public static implicit operator Pipeline(PipelineStateVK state)
        {
            return state._pipeline;
        }

        internal PipelineStateVK BaseState { get; }
    }
}
