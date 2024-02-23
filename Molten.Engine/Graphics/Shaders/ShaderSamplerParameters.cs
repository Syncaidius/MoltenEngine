using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Molten.Graphics;

public class ShaderSamplerParameters : IEquatable<ShaderSamplerParameters>
{
    /// <summary>Gets or sets the method to use for resolving a U texture coordinate that is outside the 0 to 1 range.</summary>
    public SamplerAddressMode AddressU;

    /// <summary>Gets or sets the method to use for resolving a V texture coordinate that is outside the 0 to 1 range.</summary>
    public SamplerAddressMode AddressV;

    /// <summary>Gets or sets the method to use for resolving a W texture coordinate that is outside the 0 to 1 range.</summary>
    public SamplerAddressMode AddressW;

    /// <summary>Border color to use if <see cref="SamplerAddressMode.Border"/> is specified 
    /// for AddressU, AddressV, or AddressW. Range must be between 0.0 and 1.0 inclusive.</summary>
    public Color4 BorderColor;

    /// <summary>A function that compares sampled data against existing sampled data.</summary>
    public ComparisonMode Comparison;

    /// <summary>Gets or sets the filtering method to use for minification (shrink) sampling.</summary>
    public SamplerFilter MinFilter;

    /// <summary>Gets or sets the filtering method to use for magnifcation (enlarge) sampling.</summary>
    public SamplerFilter MagFilter;

    /// <summary>Gets or sets the filtering method to use when mip-level sampling.</summary>
    public SamplerFilter MipFilter;

    /// <summary>The max texture anisotrophy when performing anisotropic filtering. Valid values are between 1 and 16.
    /// <para>A value of 0 will disable anisotrophic filtering.</para></summary>
    public uint MaxAnisotropy;

    /// <summary>Gets or sets whether or not the sampler is used for comparison sampling.</summary>
    public bool IsComparison;

    /// <summary>Upper end of the mipmap range to clamp access to, where 0 is the largest
    ///     and most detailed mipmap level and any level higher than that is less detailed.
    ///     This value must be greater than or equal to MinLOD. To have no upper limit
    ///     on LOD set this to a large value such as D3D11_FLOAT32_MAX.</summary>
    public float MaxMipMapLod;

    /// <summary>Lower end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level 
    /// and any level higher than that is less detailed.</summary>
    public float MinMipMapLod;

    /// <summary>Gets or sets the offset from the calculated mipmap level. For example, if Direct3D calculates 
    /// that a texture should be sampled at mipmap level 3 and MipLODBias is 2, then 
    /// the texture will be sampled at mipmap level 5.</summary>
    public float LodBias;

    /// <summary>
    /// The sampelr preset.
    /// </summary>
    public SamplerPreset Preset;

    /// <summary>
    /// Gets the <see cref="ShaderSampler"/> that is linked to the current parameters. 
    /// <para>This is populated internally by the shader compiler to allow common sampler instances to be shared between passes.</para>
    /// </summary>
    [JsonIgnore]
    public ShaderSampler LinkedSampler { get; internal set; }

    public ShaderSamplerParameters()
    {
        ApplyPreset(SamplerPreset.Default);
    }

    public ShaderSamplerParameters(SamplerPreset preset)
    {
        ApplyPreset(preset);
    }

    public void ApplyPreset(SamplerPreset preset)
    {
        Preset = preset;

        // Revert to default
        MinFilter = SamplerFilter.Linear;
        MagFilter = SamplerFilter.Linear;
        MipFilter = SamplerFilter.Linear;
        AddressU = SamplerAddressMode.Wrap;
        AddressV = SamplerAddressMode.Wrap;
        AddressW = SamplerAddressMode.Wrap;
        IsComparison = false;
        MinMipMapLod = float.MinValue;
        MaxMipMapLod = float.MaxValue;
        LodBias = 0f;
        MaxAnisotropy = 0;
        BorderColor = Color.White;
        Comparison = ComparisonMode.Never;

        // Now apply preset values.
        switch (preset)
        {
            case SamplerPreset.Clamp:
                AddressU = SamplerAddressMode.Clamp;
                AddressV = SamplerAddressMode.Clamp;
                AddressW = SamplerAddressMode.Clamp;
                break;

            case SamplerPreset.Mirror:
                AddressU = SamplerAddressMode.Mirror;
                AddressV = SamplerAddressMode.Mirror;
                AddressW = SamplerAddressMode.Mirror;
                break;

            case SamplerPreset.MirrorOnce:
                AddressU = SamplerAddressMode.MirrorOnce;
                AddressV = SamplerAddressMode.MirrorOnce;
                AddressW = SamplerAddressMode.MirrorOnce;
                break;

            case SamplerPreset.Border:
                AddressU = SamplerAddressMode.Border;
                AddressV = SamplerAddressMode.Border;
                AddressW = SamplerAddressMode.Border;
                break;
        }
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is ShaderSamplerParameters p)
            return Equals(p);

        return false;
    }

    public bool Equals(ShaderSamplerParameters other)
    {
        return AddressU == other.AddressU &&
            AddressV == other.AddressV &&
            AddressW == other.AddressW &&
            BorderColor == other.BorderColor &&
            Comparison == other.Comparison &&
            MinFilter == other.MinFilter &&
            MagFilter == other.MagFilter &&
            MipFilter == other.MipFilter &&
            MaxAnisotropy == other.MaxAnisotropy &&
            IsComparison == other.IsComparison &&
            MaxMipMapLod == other.MaxMipMapLod &&
            MinMipMapLod == other.MinMipMapLod &&
            LodBias == other.LodBias;
    }
}

public enum SamplerPreset
{
    /// <summary>The default blend mode. All address modes are set to <see cref="SamplerAddressMode.Wrap"/></summary>
    Default = 0,

    Clamp = 1,

    Mirror = 2,

    MirrorOnce = 3,

    Border = 4,
}
