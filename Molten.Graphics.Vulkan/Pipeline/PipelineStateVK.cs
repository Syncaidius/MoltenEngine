using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal unsafe class PipelineStateVK : GraphicsPipelineState
    {
        public class FaceVK : Face
        {
            internal StencilOpState Desc;
            PipelineStateVK _parent;

            internal FaceVK(PipelineStateVK parent, ref StencilOpState defaultDesc)
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

        StructKey<GraphicsPipelineCreateInfo> _info;
        Pipeline _pipeline;

        StructKey<PipelineDepthStencilStateCreateInfo> _descDepth { get; }
        DepthStateVK _depthState;
        bool _dirtyDepth = true;

        internal PipelineStateVK(GraphicsDevice device) : 
            base(device)
        {
            _info = new StructKey<GraphicsPipelineCreateInfo>();
        }

        protected override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override void GraphicsRelease()
        {
            throw new NotImplementedException();
        }

        protected override Face CreateFace(bool isFrontFace)
        {
            if (isFrontFace)
                return new FaceVK(this, ref _descDepth.Value.Front);
            else
                return new FaceVK(this, ref _descDepth.Value.Back);
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            if (_dirtyDepth)
            {
                _depthState = Device.CacheObject(_descDepth, _depthState);

                // If no matching state was found, create one.
                if (_depthState == null)
                    _depthState = Device.CacheObject(_descDepth, new DepthStateVK(Device, _descDepth));

                _info.Value.PDepthStencilState = _depthState.Desc;
                _dirtyDepth = false;
                Version++;
            }

            if (_dirtyDepth)
            {
                _descDepth.Value.Front = (FrontFace as FaceVK).Desc;
                _descDepth.Value.Back = (BackFace as FaceVK).Desc;
                _dirtyDepth = false;
                Version++;
            }

            // TODO if dirty, recreate _pipeline.

            /* TODO refactor GraphicsCommandQueue to only have a single State slot.
             *  - GraphicsCommandQueue.ApplyState() should become abstract
             *  - Each API should apply the state object in it's own way
             *  - GraphicsPipelineState will be created to contain
             *      - Vertex input layout
             *      - Depth, Rasterizer and blend states
             *      - Viewport state/info
             *      - API-specific state info
             *      - Most features listed here: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkGraphicsPipelineCreateInfo.html
             *      - In DX11 this could all be stored in a Deferred Context
             *  - Note that some parts of a pipeline state will need updating dynamically per frame or during resizes:
             *  - See: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkDynamicState.html
             *      - Viewports
             *      - Scissor rects
             *      - Stencil reference value
             *  - Each Renderable will store a GraphicsPipelineState on it
             *      - Dynamic parts will update each time the Renderable is rendered, as above
             *  - If a state is modified (e.g. a depth state was reassigned) then the state will be recreated prior to next usage.
             *  
             *  NOTE: This may 
            */
        }

        protected override RenderSurfaceBlend CreateSurfaceBlend(int index)
        {
            throw new NotImplementedException();
        }

        public override bool IsDepthEnabled
        {
            get => _descDepth.Value.DepthTestEnable;
            set
            {
                if (_descDepth.Value.DepthTestEnable != value)
                {
                    _descDepth.Value.DepthTestEnable = value;
                    _dirtyDepth = true;
                }
            }
        }

        public override bool IsStencilEnabled
        {
            get => _descDepth.Value.StencilTestEnable;
            set
            {
                if (_descDepth.Value.StencilTestEnable != value)
                {
                    _descDepth.Value.StencilTestEnable = value;
                    _dirtyDepth = true;
                }
            }
        }

        public override bool DepthWriteEnabled
        {
            get => _descDepth.Value.DepthWriteEnable;
            set
            {
                if (_descDepth.Value.DepthWriteEnable != value)
                {
                    _descDepth.Value.DepthWriteEnable = value;
                    _dirtyDepth = true;
                }
            }
        }

        public override bool DepthBoundsTestEnabled
        {
            get => _descDepth.Value.DepthBoundsTestEnable;
            set
            {
                if (_descDepth.Value.DepthBoundsTestEnable != value)
                {
                    _descDepth.Value.DepthBoundsTestEnable = value;
                    _dirtyDepth = true;
                }
            }
        }

        public override float MaxDepthBounds
        {
            get => _descDepth.Value.MaxDepthBounds;
            set
            {
                if (_descDepth.Value.MaxDepthBounds != value)
                {
                    _descDepth.Value.MaxDepthBounds = value;
                    _dirtyDepth = true;
                }
            }
        }

        public override float MinDepthBounds
        {
            get => _descDepth.Value.MinDepthBounds;
            set
            {
                if (_descDepth.Value.MinDepthBounds != value)
                {
                    _descDepth.Value.MinDepthBounds = value;
                    _dirtyDepth = true;
                }
            }
        }

        public override ComparisonFunction DepthComparison
        {
            get => (ComparisonFunction)_descDepth.Value.DepthCompareOp;
            set
            {
                CompareOp op = value.ToApi();
                if (_descDepth.Value.DepthCompareOp != op)
                {
                    _descDepth.Value.DepthCompareOp = op;
                    _dirtyDepth = true;
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
                _dirtyDepth = true;
            }
        }

        public override byte StencilWriteMask
        {
            get => (byte)(FrontFace as FaceVK).Desc.WriteMask;
            set
            {
                (FrontFace as FaceVK).Desc.WriteMask = value;
                (BackFace as FaceVK).Desc.WriteMask = value;
                _dirtyDepth = true;
            }
        }

        public override RasterizerCullingMode Cull { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int DepthBias { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override float DepthBiasClamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override RasterizerFillingMode Fill { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool IsAALineEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool IsDepthClipEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool IsFrontCounterClockwise { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool IsMultisampleEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool IsScissorEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override float SlopeScaledDepthBias { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ConservativeRasterizerMode ConservativeRaster { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override uint ForcedSampleCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool AlphaToCoverageEnable { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool IndependentBlendEnable { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override uint BlendSampleMask { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override Color4 BlendFactor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
