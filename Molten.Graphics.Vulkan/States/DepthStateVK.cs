using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    public unsafe class DepthStateVK : GraphicsDepthState
    {
        public class FaceVK : Face
        {
            internal StencilOpState Desc;
            DepthStateVK _parent;

            internal FaceVK(DepthStateVK parent, ref StencilOpState defaultDesc)
            {
                _parent = parent;
                Desc = defaultDesc;
            }

            public override ComparisonFunction Comparison
            {
                get => Desc.CompareOp.FromApi();
                set
                {
                    CompareOp func = value.ToApi();
                    if (Desc.CompareOp != func)
                    {
                        Desc.CompareOp = func;
                        _parent._dirty = true;
                    }
                }
            }

            public override DepthStencilOperation StencilPass
            {
                get => Desc.PassOp.FromApi();
                set
                {
                    StencilOp op = value.ToApi();
                    if (Desc.PassOp != op)
                    {
                        Desc.PassOp = op;
                        _parent._dirty = true;
                    }
                }
            }

            public override DepthStencilOperation StencilFail
            {
                get => Desc.FailOp.FromApi();
                set
                {
                    StencilOp op = value.ToApi();
                    if (Desc.FailOp != op)
                    {
                        Desc.FailOp = op;
                        _parent._dirty = true;
                    }
                }
            }

            public override DepthStencilOperation DepthFail
            {
                get => Desc.DepthFailOp.FromApi();
                set
                {
                    StencilOp op = value.ToApi();
                    if (Desc.DepthFailOp != op)
                    {
                        Desc.DepthFailOp = op;
                        _parent._dirty = true;
                    }
                }
            }
        }

        PipelineDepthStencilStateCreateInfo* _ptrDesc;
        bool _dirty = true;

        public DepthStateVK(GraphicsDevice device, GraphicsDepthState source) : base(device, source)
        {
            _dirty = true;
            _ptrDesc = EngineUtil.Alloc<PipelineDepthStencilStateCreateInfo>();
            _ptrDesc->SType = StructureType.PipelineDepthStencilStateCreateInfo;
        }

        protected override Face CreateFace(bool isFrontFace)
        {
            if (isFrontFace)
                return new FaceVK(this, ref _ptrDesc->Front);
            else
                return new FaceVK(this, ref _ptrDesc->Back);
        }

        protected override void OnApply(GraphicsCommandQueue context)
        {
            if (_dirty)
            {
                _ptrDesc->Front = (FrontFace as FaceVK).Desc;
                _ptrDesc->Back = (BackFace as FaceVK).Desc;
                _dirty = false;
                Version++;
            }
        }

        public override void GraphicsRelease()
        {
            EngineUtil.Free(ref _ptrDesc);
        }

        internal ref PipelineDepthStencilStateCreateInfo* Desc => ref _ptrDesc;

        public override bool IsDepthEnabled
        {
            get => _ptrDesc->DepthTestEnable;
            set
            {
                if (_ptrDesc->DepthTestEnable != value)
                {
                    _ptrDesc->DepthTestEnable = value;
                    _dirty = true;
                }
            }
        }

        public override bool IsStencilEnabled
        {
            get => _ptrDesc->StencilTestEnable;
            set
            {
                if (_ptrDesc->StencilTestEnable != value)
                {
                    _ptrDesc->StencilTestEnable = value;
                    _dirty = true;
                }
            }
        }

        public override bool DepthWriteEnabled
        {
            get => _ptrDesc->DepthWriteEnable;
            set
            {
                if (_ptrDesc->DepthWriteEnable != value)
                {
                    _ptrDesc->DepthWriteEnable = value;
                    _dirty = true;
                }
            }
        }

        public override bool DepthBoundsTestEnabled
        {
            get => _ptrDesc->DepthBoundsTestEnable;
            set
            {
                if (_ptrDesc->DepthBoundsTestEnable != value)
                {
                    _ptrDesc->DepthBoundsTestEnable = value;
                    _dirty = true;
                }
            }
        }

        public override float MaxDepthBounds
        {
            get => _ptrDesc->MaxDepthBounds;
            set
            {
                if (_ptrDesc->MaxDepthBounds != value)
                {
                    _ptrDesc->MaxDepthBounds = value;
                    _dirty = true;
                }
            }
        }

        public override float MinDepthBounds
        {
            get => _ptrDesc->MinDepthBounds;
            set
            {
                if (_ptrDesc->MinDepthBounds != value)
                {
                    _ptrDesc->MinDepthBounds = value;
                    _dirty = true;
                }
            }
        }

        public override ComparisonFunction DepthComparison
        {
            get => (ComparisonFunction)_ptrDesc->DepthCompareOp;
            set
            {
                CompareOp op = value.ToApi();
                if (_ptrDesc->DepthCompareOp != op)
                {
                    _ptrDesc->DepthCompareOp = op;
                    _dirty = true;
                }
            }
        }

        public override byte StencilReadMask
        {
            get => (byte)(FrontFace as FaceVK).Desc.CompareMask;
            set
            {
                (FrontFace as FaceVK).Desc.CompareMask = value;
                (BackFace as FaceVK).Desc.CompareMask = value;
                _dirty = true;
            }
        }

        public override byte StencilWriteMask
        {
            get => (byte)(FrontFace as FaceVK).Desc.WriteMask;
            set
            {
                (FrontFace as FaceVK).Desc.WriteMask = value;
                (BackFace as FaceVK).Desc.WriteMask = value;
                _dirty = true;
            }
        }
    }
}
