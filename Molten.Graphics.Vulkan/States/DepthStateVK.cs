using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    public unsafe class DepthStateVK : GraphicsObject
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
                        _parent._dirtyDepth = true;
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
                        _parent._dirtyDepth = true;
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
                        _parent._dirtyDepth = true;
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
                        _parent._dirtyDepth = true;
                    }
                }
            }
        }

        internal StructKey<PipelineDepthStencilStateCreateInfo> Desc { get; }
        public DepthStateVK(GraphicsDevice device, StructKey<PipelineDepthStencilStateCreateInfo> desc) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            Desc = new StructKey<PipelineDepthStencilStateCreateInfo>(desc);
            Desc.Value.SType = StructureType.PipelineDepthStencilStateCreateInfo;
        }

        public override void GraphicsRelease()
        {
            Desc.Dispose();
        }
    }
}
