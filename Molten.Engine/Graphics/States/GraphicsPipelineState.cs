using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class GraphicsPipelineState : GraphicsObject
    {
        public abstract class Face
        {
            /// <summary>
            /// Gets or sets the comparison function for the current depth-stencil facing.
            /// </summary>
            [ShaderNode(ShaderNodeParseType.Enum)]
            public abstract ComparisonFunction Comparison { get; set; }

            /// <summary>
            /// Gets or sets the stencil pass operation for the current depth-stencil facing.
            /// </summary>
            [ShaderNode(ShaderNodeParseType.Enum)]
            public abstract DepthStencilOperation StencilPass { get; set; }

            /// <summary>
            /// Gets or sets the stencil fail operation for the current depth-stencil facing.
            /// </summary>
            [ShaderNode(ShaderNodeParseType.Enum)]
            public abstract DepthStencilOperation StencilFail { get; set; }

            /// <summary>
            /// Gets or sets the depth fail operation for the current depth-stencil facing.
            /// </summary>
            [ShaderNode(ShaderNodeParseType.Enum)]
            public abstract DepthStencilOperation DepthFail { get; set; }

            /// <summary>
            /// Sets the values of the current <see cref="Face"/> to match that of another.
            /// </summary>
            /// <param name="other">The <see cref="Face"/> from which to copy values.</param>
            public void Set(Face other)
            {
                Comparison = other.Comparison;
                StencilPass = other.StencilPass;
                StencilFail = other.StencilFail;
                DepthFail = other.DepthFail;
            }
        }

        public abstract class RenderSurfaceBlend
        {
            [ShaderNode(ShaderNodeParseType.Bool)]
            public abstract bool BlendEnable { get; set; }

            [ShaderNode(ShaderNodeParseType.Bool)]
            public abstract bool LogicOpEnable { get; set; }

            [ShaderNode(ShaderNodeParseType.Enum)]
            public abstract BlendType SrcBlend { get; set; }

            [ShaderNode(ShaderNodeParseType.Enum)]
            public abstract BlendType DestBlend { get; set; }

            [ShaderNode(ShaderNodeParseType.Enum)]
            public abstract BlendOperation BlendOp { get; set; }

            [ShaderNode(ShaderNodeParseType.Enum)]
            public abstract BlendType SrcBlendAlpha { get; set; }

            [ShaderNode(ShaderNodeParseType.Enum)]
            public abstract BlendType DestBlendAlpha { get; set; }

            [ShaderNode(ShaderNodeParseType.Enum)]
            public abstract BlendOperation BlendOpAlpha { get; set; }

            [ShaderNode(ShaderNodeParseType.Enum)]
            public abstract LogicOperation LogicOp { get; set; }

            [ShaderNode(ShaderNodeParseType.Enum)]
            public abstract ColorWriteFlags RenderTargetWriteMask { get; set; }

            public void Set(RenderSurfaceBlend other)
            {
                BlendEnable = other.BlendEnable;
                LogicOpEnable = other.LogicOpEnable;
                SrcBlend = other.SrcBlend;
                DestBlend = other.DestBlend;
                BlendOp = other.BlendOp;
                SrcBlendAlpha = other.SrcBlendAlpha;
                DestBlendAlpha = other.DestBlendAlpha;
                BlendOpAlpha = other.BlendOpAlpha;
                LogicOp = other.LogicOp;
                RenderTargetWriteMask = other.RenderTargetWriteMask;
            }
        }

        RenderSurfaceBlend[] _surfaceBlends;

        protected GraphicsPipelineState(GraphicsDevice device) :
            base(device, GraphicsBindTypeFlags.Input)
        {
            _surfaceBlends = new RenderSurfaceBlend[device.Adapter.Capabilities.PixelShader.MaxOutResources];
            for (int i = 0; i < _surfaceBlends.Length; i++)
                _surfaceBlends[i] = CreateSurfaceBlend(i);

            FrontFace = CreateFace(true);
            BackFace = CreateFace(false);

            device.StatePresets.ApplyPreset(this, PipelineStatePreset.Default);
        }

        [ShaderNode(ShaderNodeParseType.Enum)]
        public abstract RasterizerCullingMode Cull { get; set; }

        [ShaderNode(ShaderNodeParseType.Int32)]
        public abstract int DepthBias { get; set; }

        [ShaderNode(ShaderNodeParseType.Float)]
        public abstract float DepthBiasClamp { get; set; }

        [ShaderNode(ShaderNodeParseType.Enum)]
        public abstract RasterizerFillingMode Fill { get; set; }

        /// <summary>
        /// Gets or sets whether or not anti-aliased line rasterization is enabled.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool IsAALineEnabled { get; set; }

        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool IsDepthClipEnabled { get; set; }

        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool IsFrontCounterClockwise { get; set; }

        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool IsMultisampleEnabled { get; set; }

        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool IsScissorEnabled { get; set; }

        [ShaderNode(ShaderNodeParseType.Float)]
        public abstract float SlopeScaledDepthBias { get; set; }

        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract ConservativeRasterizerMode ConservativeRaster { get; set; }

        [ShaderNode(ShaderNodeParseType.UInt32)]
        public abstract uint ForcedSampleCount { get; set; }

        protected abstract RenderSurfaceBlend CreateSurfaceBlend(int index);

        internal RenderSurfaceBlend GetSurfaceBlendState(int index)
        {
            return _surfaceBlends[index];
        }

        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool AlphaToCoverageEnable { get; set; }

        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool IndependentBlendEnable { get; set; }

        /// <summary>
        /// Gets or sets a render target blend description at the specified index.
        /// </summary>
        /// <param name="rtIndex">The render target/surface blend index.</param>
        /// <returns></returns>
        public RenderSurfaceBlend this[int rtIndex] => _surfaceBlends[rtIndex];

        /// <summary>
        /// Gets or sets the blend sample mask.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.UInt32)]
        public uint BlendSampleMask { get; set; }

        /// <summary>
        /// Gets or sets the blend factor.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Color)]
        public Color4 BlendFactor { get; set; }

        /// <summary>
        /// Invoked when a new <see cref="Face"/> instance is required for the current <see cref="GraphicsPipelineState"/>.
        /// </summary>
        /// <param name="isFrontFace"></param>
        /// <returns></returns>
        protected abstract Face CreateFace(bool isFrontFace);

        private static bool CompareOperation(Face op, Face other)
        {
            return op.Comparison == other.Comparison &&
                op.DepthFail == other.DepthFail &&
                op.StencilFail == other.StencilFail &&
                op.StencilPass == other.StencilPass;
        }

        /// <summary>
        /// Gets or sets whether or not depth-mapping is enabled.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool IsDepthEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether or not stencil-mapping is enabled.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool IsStencilEnabled { get; set; }

        /// <summary>
        /// Gets or sets the depth write flags.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool DepthWriteEnabled { get; set; }

        /// <summary>
        /// Gets or sets the depth comparison function.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Enum)]
        public abstract ComparisonFunction DepthComparison { get; set; }

        /// <summary>
        /// Gets or sets the stencil read mask.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract byte StencilReadMask { get; set; }

        /// <summary>
        /// Gets or sets the stencil write mask.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Byte)]
        public abstract byte StencilWriteMask { get; set; }

        /// <summary>
        /// Gets or sets whether or not depth bounds testing is enabled. 
        /// <para>This property should throw a <see cref="NotSupportedException"/>, 
        /// if <see cref="GraphicsCapabilities.DepthBoundsTesting"/> is false in <see cref="GraphicsDevice.Adapter"/>.</para>
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public abstract bool DepthBoundsTestEnabled { get; set; }

        /// <summary>
        /// Gets or sets the maximum bounds during depth bounds testing.
        /// <para>This value may be ignored if <see cref="DepthBoundsTestEnabled"/> is false, or bounds testing is unsupported.</para>
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Float)]
        public abstract float MaxDepthBounds { get; set; }

        /// <summary>
        /// Gets or sets the minimum bounds during depth bounds testing.
        /// <para>This value may be ignored if <see cref="DepthBoundsTestEnabled"/> is false, or bounds testing is unsupported.</para>
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Float)]
        public abstract float MinDepthBounds { get; set; }

        /// <summary>Gets the description for the front-face depth operation description.</summary>
        [ShaderNode(ShaderNodeParseType.Object)]
        public Face FrontFace { get; }

        /// <summary>Gets the description for the back-face depth operation description.</summary>
        [ShaderNode(ShaderNodeParseType.Object)]
        public Face BackFace { get; }

        /// <summary>Gets or sets the stencil reference value. The default value is 0.</summary>
        [ShaderNode(ShaderNodeParseType.UInt32)]
        public uint StencilReference { get; set; }

        /// <summary>
        /// Gets or sets the depth write permission. the default value is <see cref="GraphicsDepthWritePermission.Enabled"/>.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Enum)]
        public GraphicsDepthWritePermission WritePermission { get; set; } = GraphicsDepthWritePermission.Enabled;

        public int SurfaceBlendCount => _surfaceBlends.Length;
    }
}
