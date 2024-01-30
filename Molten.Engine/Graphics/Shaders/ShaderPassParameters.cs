using Newtonsoft.Json;

namespace Molten.Graphics;

public partial struct ShaderPassParameters
{
    public const int MAX_SURFACES = 8;

    public struct Face
    {
        /// <summary>
        /// Gets or sets the comparison function for the current depth-stencil facing.
        /// </summary>
        public ComparisonFunction Comparison;

        /// <summary>
        /// Gets or sets the stencil pass operation for the current depth-stencil facing.
        /// </summary>
        public DepthStencilOperation StencilPass;

        /// <summary>
        /// Gets or sets the stencil fail operation for the current depth-stencil facing.
        /// </summary>
        public DepthStencilOperation StencilFail;

        /// <summary>
        /// Gets or sets the depth fail operation for the current depth-stencil facing.
        /// </summary>
        public DepthStencilOperation DepthFail;

        /// <summary>Gets or sets the stencil reference value. The default value is 0.</summary>
        public uint StencilReference { get; set; }
    }

    public struct SurfaceBlend
    {
        public bool BlendEnable;

        public bool LogicOpEnable;

        public BlendType SrcBlend;

        public BlendType DestBlend;

        public BlendOperation BlendOp;

        public BlendType SrcBlendAlpha;

        public BlendType DestBlendAlpha;

        public BlendOperation BlendOpAlpha;

        public LogicOperation LogicOp;

        public ColorWriteFlags RenderTargetWriteMask;
    }

    [JsonProperty("blend")]
    public BlendPreset Blend { get; set; }

    [JsonProperty("rasterizer")]
    public RasterizerPreset Rasterizer { get; set; }

    [JsonProperty("depth")]
    public DepthStencilPreset Depth { get; set; }

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
    public bool AlphaToCoverageEnable;

    public bool IndependentBlendEnable;

    /// <summary>
    /// Gets or sets the blend sample mask.
    /// </summary>
    public uint BlendSampleMask;

    /// <summary>
    /// Gets or sets the blend factor.
    /// </summary>
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
    public bool IsDepthEnabled;

    /// <summary>
    /// Gets or sets whether or not stencil-mapping is enabled.
    /// </summary>
    public bool IsStencilEnabled;

    /// <summary>
    /// Gets or sets the depth write flags.
    /// </summary>
    public bool DepthWriteEnabled;

    /// <summary>
    /// Gets or sets the depth comparison function.
    /// </summary>
    public ComparisonFunction DepthComparison;

    /// <summary>
    /// Gets or sets the stencil read mask.
    /// </summary>
    public byte StencilReadMask;

    /// <summary>
    /// Gets or sets the stencil write mask.
    /// </summary>
    public byte StencilWriteMask;

    /// <summary>
    /// Gets or sets whether or not depth bounds testing is enabled. 
    /// <para>This property should throw a <see cref="NotSupportedException"/>, 
    /// if <see cref="GraphicsCapabilities.DepthBoundsTesting"/> is false in <see cref="GraphicsDevice.Adapter"/>.</para>
    /// </summary>
    public bool DepthBoundsTestEnabled;

    /// <summary>
    /// Gets or sets the maximum bounds during depth bounds testing.
    /// <para>This value may be ignored if <see cref="DepthBoundsTestEnabled"/> is false, or bounds testing is unsupported.</para>
    /// </summary>
    public float MaxDepthBounds;

    /// <summary>
    /// Gets or sets the minimum bounds during depth bounds testing.
    /// <para>This value may be ignored if <see cref="DepthBoundsTestEnabled"/> is false, or bounds testing is unsupported.</para>
    /// </summary>
    public float MinDepthBounds;

    /// <summary>Gets the description for the front-face depth operation description.</summary>
    public Face DepthFrontFace;

    /// <summary>Gets the description for the back-face depth operation description.</summary>
    public Face DepthBackFace;
    #endregion

    #region Rasterizer Parameters
    public RasterizerCullingMode Cull;

    public int DepthBias;

    public float DepthBiasClamp;

    public bool DepthBiasEnabled;

    public RasterizerFillingMode Fill;

    public float LineWidth; 

    /// <summary>
    /// Gets or sets whether or not anti-aliased line rasterization is enabled.
    /// </summary>
    public bool IsAALineEnabled;

    public bool IsDepthClipEnabled;

    public bool IsFrontCounterClockwise;

    public bool IsMultisampleEnabled;

    /// <summary>
    /// If true, primitives are discarded immediately before the rasterization stage.
    /// </summary>
    public bool RasterizerDiscardEnabled;

    /// <summary>
    /// Gets or sets whether scissor-test rasterization is enabled. 
    /// <para>This involves skipping the rasterization of pixels outside defined scissor bounds.</para>
    /// </summary>
    public bool IsScissorEnabled;

    public float SlopeScaledDepthBias;

    public ConservativeRasterizerMode ConservativeRaster;

    public uint ForcedSampleCount;

    public PrimitiveTopology Topology;
    #endregion
}
