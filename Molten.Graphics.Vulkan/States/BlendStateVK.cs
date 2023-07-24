using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe class BlendStateVK : GraphicsObject, IEquatable<BlendStateVK>, IEquatable<PipelineColorBlendStateCreateInfo>
    {
        PipelineColorBlendStateCreateInfo* _desc;

        public BlendStateVK(GraphicsDevice device, ref ShaderPassParameters parameters) :
            base(device)
        {
            _desc = EngineUtil.Alloc<PipelineColorBlendStateCreateInfo>();
            _desc[0] = new PipelineColorBlendStateCreateInfo()
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                LogicOp = parameters.Surface0.LogicOp.ToApi(),
                LogicOpEnable = parameters.Surface0.LogicOpEnable,
                AttachmentCount = ShaderPassParameters.MAX_SURFACES,
                Flags = PipelineColorBlendStateCreateFlags.None,// See: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkPipelineColorBlendStateCreateFlagBits.html
                PAttachments = EngineUtil.AllocArray<PipelineColorBlendAttachmentState>(_desc->AttachmentCount),
                PNext = null,
            };

            Color4 blendConsts = parameters.BlendFactor;
            _desc->BlendConstants[0] = blendConsts.R;
            _desc->BlendConstants[1] = blendConsts.G;
            _desc->BlendConstants[2] = blendConsts.B;
            _desc->BlendConstants[3] = blendConsts.A;

            for (uint i = 0; i < _desc->AttachmentCount; i++)
            {
                ref PipelineColorBlendAttachmentState at = ref _desc->PAttachments[i];
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

        public override bool Equals(object obj) => obj switch
        {
            BlendStateVK val => Equals(val._desc[0]),
            PipelineColorBlendStateCreateInfo val => Equals(val),
            _ => base.Equals(obj)
        };

        public bool Equals(BlendStateVK other)
        {
            return Equals(*other._desc);
        }

        public bool Equals(PipelineColorBlendStateCreateInfo other)
        {
            if (_desc->Flags != other.Flags
            || _desc->LogicOp != other.LogicOp
            || _desc->LogicOpEnable.Value != other.LogicOpEnable.Value
            || _desc->BlendConstants[0] != other.BlendConstants[0]
            || _desc->BlendConstants[1] != other.BlendConstants[1]
            || _desc->BlendConstants[2] != other.BlendConstants[2]
            || _desc->BlendConstants[3] != other.BlendConstants[3]
            || _desc->AttachmentCount != other.AttachmentCount)
                return false;

            // Check if blend attachments are equal.
            for (uint i = 0; i < _desc->AttachmentCount; i++)
            {
                ref PipelineColorBlendAttachmentState att = ref _desc->PAttachments[i];
                ref PipelineColorBlendAttachmentState otherAtt = ref other.PAttachments[i];

                if (att.BlendEnable.Value != otherAtt.BlendEnable.Value
                    || att.SrcColorBlendFactor != otherAtt.SrcColorBlendFactor
                    || att.DstColorBlendFactor != otherAtt.DstColorBlendFactor
                    || att.ColorBlendOp != otherAtt.ColorBlendOp
                    || att.SrcAlphaBlendFactor != otherAtt.SrcAlphaBlendFactor
                    || att.DstAlphaBlendFactor != otherAtt.DstAlphaBlendFactor
                    || att.AlphaBlendOp != otherAtt.AlphaBlendOp
                    || att.ColorWriteMask != otherAtt.ColorWriteMask)
                    return false;
            }

            return true;
        }

        protected override void OnGraphicsRelease()
        {
            EngineUtil.Free(ref _desc->PAttachments);
            EngineUtil.Free(ref _desc);
        }

        internal PipelineColorBlendStateCreateInfo* Desc => _desc;
    }
}
