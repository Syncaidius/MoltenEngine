using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public partial struct ShaderPassParameters
    {
        public const int MAX_SURFACES = 8;

        public struct Face
        {
            /// <summary>
            /// Gets or sets the comparison function for the current depth-stencil facing.
            /// </summary>
            [ShaderNode(ShaderNodeParseType.Enum)]
            public ComparisonFunction Comparison;

            /// <summary>
            /// Gets or sets the stencil pass operation for the current depth-stencil facing.
            /// </summary>
            [ShaderNode(ShaderNodeParseType.Enum)]
            public DepthStencilOperation StencilPass;

            /// <summary>
            /// Gets or sets the stencil fail operation for the current depth-stencil facing.
            /// </summary>
            [ShaderNode(ShaderNodeParseType.Enum)]
            public DepthStencilOperation StencilFail;

            /// <summary>
            /// Gets or sets the depth fail operation for the current depth-stencil facing.
            /// </summary>
            [ShaderNode(ShaderNodeParseType.Enum)]
            public DepthStencilOperation DepthFail;

            /// <summary>Gets or sets the stencil reference value. The default value is 0.</summary>
            [ShaderNode(ShaderNodeParseType.UInt32)]
            public uint StencilReference { get; set; }
        }

        public struct SurfaceBlend
        {
            [ShaderNode(ShaderNodeParseType.Bool)]
            public bool BlendEnable;

            [ShaderNode(ShaderNodeParseType.Bool)]
            public bool LogicOpEnable;

            [ShaderNode(ShaderNodeParseType.Enum)]
            public BlendType SrcBlend;

            [ShaderNode(ShaderNodeParseType.Enum)]
            public BlendType DestBlend;

            [ShaderNode(ShaderNodeParseType.Enum)]
            public BlendOperation BlendOp;

            [ShaderNode(ShaderNodeParseType.Enum)]
            public BlendType SrcBlendAlpha;

            [ShaderNode(ShaderNodeParseType.Enum)]
            public BlendType DestBlendAlpha;

            [ShaderNode(ShaderNodeParseType.Enum)]
            public BlendOperation BlendOpAlpha;

            [ShaderNode(ShaderNodeParseType.Enum)]
            public LogicOperation LogicOp;

            [ShaderNode(ShaderNodeParseType.Enum)]
            public ColorWriteFlags RenderTargetWriteMask;
        }

        /// <summary>
        /// The number of X compute groups to use when dispatching a compute <see cref="HlslPass"/>. Ignored for render <see cref="HlslPass"/>es.
        /// </summary>
        public uint GroupsX;

        /// <summary>
        /// The number of Y compute groups to use when dispatching a compute <see cref="HlslPass"/>. Ignored for render <see cref="HlslPass"/>es.
        /// </summary>
        public uint GroupsY;

        /// <summary>
        /// The number of Z compute groups to use when dispatching a compute <see cref="HlslPass"/>. Ignored for render <see cref="HlslPass"/>es.
        /// </summary>
        public uint GroupsZ;

        /// <summary>
        /// The blend state for the output surface at slot 0, if any.
        /// </summary>
        public SurfaceBlend Surface0;

        /// <summary>
        /// The blend state for the output surface at slot 1, if any.
        /// </summary>
        public SurfaceBlend Surface1;

        /// <summary>
        /// The blend state for the output surface at slot 2, if any.
        /// </summary>
        public SurfaceBlend Surface2;

        /// <summary>
        /// The blend state for the output surface at slot 3, if any.
        /// </summary>
        public SurfaceBlend Surface3;

        /// <summary>
        /// The blend state for the output surface at slot 4, if any.
        /// </summary>
        public SurfaceBlend Surface4;

        /// <summary>
        /// The blend state for the output surface at slot 5, if any.
        /// </summary>
        public SurfaceBlend Surface5;

        /// <summary>
        /// The blend state for the output surface at slot 6, if any.
        /// </summary>
        public SurfaceBlend Surface6;

        /// <summary>
        /// The blend state for the output surface at slot 7, if any.
        /// </summary>
        public SurfaceBlend Surface7;

        public ShaderPassParameters(GraphicsStatePreset preset, PrimitiveTopology topology)
        {
            ApplyPreset(preset);
            Topology = topology;
        }

        #region Blend parameters
        [ShaderNode(ShaderNodeParseType.Bool)]
        public bool AlphaToCoverageEnable;

        [ShaderNode(ShaderNodeParseType.Bool)]
        public bool IndependentBlendEnable;

        /// <summary>
        /// Gets or sets the blend sample mask.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.UInt32)]
        public uint BlendSampleMask;

        /// <summary>
        /// Gets or sets the blend factor.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Color)]
        public Color4 BlendFactor;

        /// <summary>
        /// Gets or sets a <see cref="SurfaceBlend"/> at the specified index.
        /// </summary>
        public SurfaceBlend this[uint index]
        {
            get => this[(int)index];            
            set => this[(int)index] = value;
        }
        /// <summary>
        /// Gets or sets a <see cref="SurfaceBlend"/> at the specified index.
        /// </summary>
        public SurfaceBlend this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Surface0;
                    case 1: return Surface1;
                    case 2: return Surface2;
                    case 3: return Surface3;
                    case 4: return Surface4;
                    case 5: return Surface5;
                    case 6: return Surface6;
                    case 7: return Surface7;
                    default:
                        if(index < 0)
                            throw new IndexOutOfRangeException($"surface index cannot be less than 0.");
                        else
                            throw new IndexOutOfRangeException($"surface index cannot be greater than GraphicsStateParameters.MAX_SURFACES constant, which is {MAX_SURFACES}.");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: Surface0 = value; break;
                    case 1: Surface1 = value; break;
                    case 2: Surface2 = value; break;
                    case 3: Surface3 = value; break;
                    case 4: Surface4 = value; break;
                    case 5: Surface5 = value; break;
                    case 6: Surface6 = value; break;
                    case 7: Surface7 = value; break;
                    default:
                        if (index < 0)
                            throw new IndexOutOfRangeException($"surface index cannot be less than 0.");
                        else
                            throw new IndexOutOfRangeException($"surface index cannot be greater than GraphicsStateParameters.MAX_SURFACES constant, which is {MAX_SURFACES}.");
                }
            }
        }
        #endregion

        #region Depth-Stencil Parameters
        /// <summary>
        /// Gets or sets whether or not depth-mapping is enabled.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public bool IsDepthEnabled;

        /// <summary>
        /// Gets or sets whether or not stencil-mapping is enabled.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public bool IsStencilEnabled;

        /// <summary>
        /// Gets or sets the depth write flags.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public bool DepthWriteEnabled;

        /// <summary>
        /// Gets or sets the depth comparison function.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Enum)]
        public ComparisonFunction DepthComparison;

        /// <summary>
        /// Gets or sets the stencil read mask.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public byte StencilReadMask;

        /// <summary>
        /// Gets or sets the stencil write mask.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Byte)]
        public byte StencilWriteMask;

        /// <summary>
        /// Gets or sets whether or not depth bounds testing is enabled. 
        /// <para>This property should throw a <see cref="NotSupportedException"/>, 
        /// if <see cref="GraphicsCapabilities.DepthBoundsTesting"/> is false in <see cref="GraphicsDevice.Adapter"/>.</para>
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public bool DepthBoundsTestEnabled;

        /// <summary>
        /// Gets or sets the maximum bounds during depth bounds testing.
        /// <para>This value may be ignored if <see cref="DepthBoundsTestEnabled"/> is false, or bounds testing is unsupported.</para>
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Float)]
        public float MaxDepthBounds;

        /// <summary>
        /// Gets or sets the minimum bounds during depth bounds testing.
        /// <para>This value may be ignored if <see cref="DepthBoundsTestEnabled"/> is false, or bounds testing is unsupported.</para>
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Float)]
        public float MinDepthBounds;

        /// <summary>Gets the description for the front-face depth operation description.</summary>
        [ShaderNode(ShaderNodeParseType.Object)]
        public Face DepthFrontFace;

        /// <summary>Gets the description for the back-face depth operation description.</summary>
        [ShaderNode(ShaderNodeParseType.Object)]
        public Face DepthBackFace;
        #endregion

        #region Rasterizer Parameters
        [ShaderNode(ShaderNodeParseType.Enum)]
        public RasterizerCullingMode Cull;

        [ShaderNode(ShaderNodeParseType.Int32)]
        public int DepthBias;

        [ShaderNode(ShaderNodeParseType.Float)]
        public float DepthBiasClamp;

        [ShaderNode(ShaderNodeParseType.Bool)]
        public bool DepthBiasEnabled;

        [ShaderNode(ShaderNodeParseType.Enum)]
        public RasterizerFillingMode Fill;

        [ShaderNode(ShaderNodeParseType.Float)]
        public float LineWidth; 

        /// <summary>
        /// Gets or sets whether or not anti-aliased line rasterization is enabled.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public bool IsAALineEnabled;

        [ShaderNode(ShaderNodeParseType.Bool)]
        public bool IsDepthClipEnabled;

        [ShaderNode(ShaderNodeParseType.Bool)]
        public bool IsFrontCounterClockwise;

        [ShaderNode(ShaderNodeParseType.Bool)]
        public bool IsMultisampleEnabled;

        /// <summary>
        /// If true, primitives are discarded immediately before the rasterization stage.
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public bool RasterizerDiscardEnabled;

        /// <summary>
        /// Gets or sets whether scissor-test rasterization is enabled. 
        /// <para>This involves skipping the rasterization of pixels outside defined scissor bounds.</para>
        /// </summary>
        [ShaderNode(ShaderNodeParseType.Bool)]
        public bool IsScissorEnabled;

        [ShaderNode(ShaderNodeParseType.Float)]
        public float SlopeScaledDepthBias;

        [ShaderNode(ShaderNodeParseType.Bool)]
        public ConservativeRasterizerMode ConservativeRaster;

        [ShaderNode(ShaderNodeParseType.UInt32)]
        public uint ForcedSampleCount;

        [ShaderNode(ShaderNodeParseType.Enum)]
        public PrimitiveTopology Topology;
        #endregion
    }
}
