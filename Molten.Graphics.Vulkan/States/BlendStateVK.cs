using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe class BlendStateVK : GraphicsObject
    {
        internal StructKey<PipelineColorBlendStateCreateInfo> Desc { get; }

        public BlendStateVK(GraphicsDevice device, ref ShaderPassParameters parameters) :
            base(device)
        {
            Desc = new StructKey<PipelineColorBlendStateCreateInfo>();
            ref PipelineColorBlendStateCreateInfo bDesc = ref Desc.Value;
            bDesc.Flags = PipelineColorBlendStateCreateFlags.None; // See: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkPipelineColorBlendStateCreateFlagBits.html

            Color4 blendConsts = parameters.BlendFactor;
            bDesc.SType = StructureType.PipelineColorBlendStateCreateInfo;
            bDesc.PNext = null;
            bDesc.BlendConstants[0] = blendConsts.R;
            bDesc.BlendConstants[1] = blendConsts.G;
            bDesc.BlendConstants[2] = blendConsts.B;
            bDesc.BlendConstants[3] = blendConsts.A;
            bDesc.LogicOp = parameters.Surface0.LogicOp.ToApi();
            bDesc.LogicOpEnable = parameters.Surface0.LogicOpEnable;
            bDesc.AttachmentCount = ShaderPassParameters.MAX_SURFACES;
            bDesc.PAttachments = EngineUtil.AllocArray<PipelineColorBlendAttachmentState>(bDesc.AttachmentCount);

            for (uint i = 0; i < bDesc.AttachmentCount; i++)
            {
                ref PipelineColorBlendAttachmentState at = ref bDesc.PAttachments[i];
                ShaderPassParameters.SurfaceBlend sBlend = parameters[i];

                at.BlendEnable = sBlend.BlendEnable;
                at.SrcColorBlendFactor = sBlend.SrcBlend.ToApi();
                at.DstColorBlendFactor = sBlend.DestBlend.ToApi();
                at.ColorBlendOp = sBlend.BlendOp.ToApi();
                at.SrcAlphaBlendFactor = sBlend.SrcBlendAlpha.ToApi();
                at.DstAlphaBlendFactor = sBlend.DestBlendAlpha.ToApi();
                at.AlphaBlendOp = sBlend.BlendOpAlpha.ToApi();
                at.ColorWriteMask = sBlend.RenderTargetWriteMask.ToApi();
            }
        }

        protected override void OnGraphicsRelease()
        {
            Desc.Dispose();
        }
    }
}
