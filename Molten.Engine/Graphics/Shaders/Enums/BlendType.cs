namespace Molten.Graphics
{
    /// <summary>
    /// Represents a blend type to be used by a shader.
    /// </summary>
    public enum BlendType
    {
        /// <summary>
        /// Indicates that the blend type is invalid.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Sets the blend factor to 0.
        /// </summary>
        Zero = 1,

        /// <summary>
        /// Sets the blend factor to 1.
        /// </summary>
        One = 2,

        /// <summary>
        /// Uses the source color.
        /// </summary>
        SrcColor = 3,

        /// <summary>
        /// Uses the inverse of the source color.
        /// </summary>
        InvSrcColor = 4,

        /// <summary>
        /// Uses the source alpha.
        /// </summary>
        SrcAlpha = 5,

        /// <summary>
        /// Uses the inverse of the source alpha.
        /// </summary>
        InvSrcAlpha = 6,

        /// <summary>
        /// Uses the destination alpha.
        /// </summary>
        DestAlpha = 7,

        /// <summary>
        /// Uses the inverse of the destination alpha.
        /// </summary>
        InvDestAlpha = 8,

        /// <summary>
        /// Uses the destination color.
        /// </summary>
        DestColor = 9,

        /// <summary>
        /// Uses the inverse of the destination color.
        /// </summary>
        InvDestColor = 10,

        /// <summary>
        /// Uses the alpha value of the source color, clamped to [0, 1].
        /// </summary>
        SrcAlphaSat = 11,

        /// <summary>
        /// Uses the blend factor.
        /// </summary>
        BlendFactor = 14,

        /// <summary>
        /// Uses the inverse of the blend factor.
        /// </summary>
        InvBlendFactor = 15,

        /// <summary>
        /// Uses the second source color.
        /// </summary>
        Src1Color = 16,

        /// <summary>
        /// Uses the inverse of the second source color.
        /// </summary>
        InvSrc1Color = 17,

        /// <summary>
        /// Uses the alpha value of the second source color.
        /// </summary>
        Src1Alpha = 18,

        /// <summary>
        /// Uses the inverse of the alpha value of the second source color.
        /// </summary>
        InvSrc1Alpha = 19,

        /// <summary>
        /// Uses the alpha value of the blend factor.
        /// </summary>
        BlendFactorAlpha = 256,

        /// <summary>
        /// Uses the inverse of the alpha value of the blend factor.
        /// </summary>
        InvBlendFactorAlpha = 257,
    }
}
