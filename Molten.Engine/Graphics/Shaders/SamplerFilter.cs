namespace Molten.Graphics
{
    //
    // Summary:
    //     Filtering options during texture sampling.
    //
    // Remarks:
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>         Note??If you use different filter types for min versus mag filter, undefined 
    /// behavior occurs in certain cases where the choice between whether magnification 
    /// or minification happens is ambiguous.To prevent this undefined behavior, use
    /// filter modes that use similar filter operations for both min and mag (or use
    ///  anisotropic filtering, which avoids the issue as well).During texture sampling,
    /// one or more texels are read and combined (this is calling filtering) to produce
    /// a single value. Point sampling reads a single texel while linear sampling reads
    /// two texels (endpoints) and linearly interpolates a third value between the endpoints.HLSL
    /// texture-sampling functions also support comparison filtering during texture sampling.
    /// Comparison filtering compares each sampled texel against a comparison value.
    /// 
    /// The boolean result is blended the same way that normal texture filtering is blended.You
    /// can use HLSL intrinsic texture-sampling functions that implement texture filtering
    /// only or companion functions that use texture filtering with comparison filtering.
    /// Texture Sampling FunctionTexture Sampling Function with Comparison Filtering
    /// samplesamplecmp or samplecmplevelzero ?Comparison filters only work with textures
    /// that have the following DXGI formats: R32_FLOAT_X8X24_TYPELESS, R32_FLOAT, R24_UNORM_X8_TYPELESS,
    /// R16_UNORM.</remarks>
    public enum SamplerFilter
    {
        /// <summary>Use point sampling for minification, magnification, and mip-level sampling.</summary>
        Point = 0,

        /// <summary>Use point sampling for minification and magnification; use linear interpolation
        /// for mip-level sampling.</summary>
        Linear = 1,
    }
}
