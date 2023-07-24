using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public unsafe class DepthStateVK : GraphicsObject, IEquatable<DepthStateVK>, IEquatable<PipelineDepthStencilStateCreateInfo>
    {
        PipelineDepthStencilStateCreateInfo* _desc;

        public DepthStateVK(GraphicsDevice device, ref ShaderPassParameters parameters) :
            base(device)
        {
            _desc = EngineUtil.Alloc<PipelineDepthStencilStateCreateInfo>();
            _desc[0] = new PipelineDepthStencilStateCreateInfo()
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = parameters.IsDepthEnabled,
                StencilTestEnable = parameters.IsStencilEnabled,
                DepthWriteEnable = parameters.DepthWriteEnabled,
                DepthBoundsTestEnable = parameters.DepthBoundsTestEnabled,
                MaxDepthBounds = parameters.MaxDepthBounds,
                MinDepthBounds = parameters.MinDepthBounds,
                DepthCompareOp = parameters.DepthComparison.ToApi(),
                PNext = null,
                Front = new StencilOpState()
                {
                    CompareMask = parameters.StencilReadMask,
                    WriteMask = parameters.StencilWriteMask,
                    CompareOp = parameters.DepthFrontFace.Comparison.ToApi(),
                    DepthFailOp = parameters.DepthFrontFace.DepthFail.ToApi(),
                    FailOp = parameters.DepthFrontFace.StencilFail.ToApi(),
                    PassOp = parameters.DepthFrontFace.StencilPass.ToApi(),
                    Reference = parameters.DepthFrontFace.StencilReference
                },
                Back = new StencilOpState()
                {
                    CompareMask = parameters.StencilWriteMask,
                    WriteMask = parameters.StencilWriteMask,
                    CompareOp = parameters.DepthBackFace.Comparison.ToApi(),
                    DepthFailOp = parameters.DepthBackFace.DepthFail.ToApi(),
                    FailOp = parameters.DepthBackFace.StencilFail.ToApi(),
                    PassOp = parameters.DepthBackFace.StencilPass.ToApi(),
                    Reference = parameters.DepthFrontFace.StencilReference,
                }
            };
        }

        private bool FaceEqual(ref StencilOpState a, ref StencilOpState b)
        {
            return a.CompareMask == b.CompareMask
                && a.WriteMask == b.WriteMask
                && a.CompareOp == b.CompareOp
                && a.DepthFailOp == b.DepthFailOp
                && a.FailOp == b.FailOp
                && a.PassOp == b.PassOp
                && a.Reference == b.Reference;
        }

        public override bool Equals(object obj) => obj switch
        {
            DepthStateVK val => Equals(*val._desc),
            PipelineDepthStencilStateCreateInfo val => Equals(val),
            _ => false
        };

        public bool Equals(DepthStateVK other)
        {
            return Equals(*other._desc);
        }

        public bool Equals(PipelineDepthStencilStateCreateInfo other)
        {
            return _desc->DepthTestEnable.Value == other.DepthTestEnable.Value
                && _desc->StencilTestEnable.Value == other.StencilTestEnable.Value
                && _desc->DepthWriteEnable.Value == other.DepthWriteEnable.Value
                && _desc->DepthBoundsTestEnable.Value == other.DepthBoundsTestEnable.Value
                && _desc->MaxDepthBounds == other.MaxDepthBounds
                && _desc->MinDepthBounds == other.MinDepthBounds
                && _desc->DepthCompareOp == other.DepthCompareOp
                && FaceEqual(ref _desc->Front, ref other.Front)
                && FaceEqual(ref _desc->Back, ref other.Back);
        }

        protected override void OnGraphicsRelease()
        {
            EngineUtil.Free(ref _desc);
        }

        internal PipelineDepthStencilStateCreateInfo* Desc => _desc;
    }
}
