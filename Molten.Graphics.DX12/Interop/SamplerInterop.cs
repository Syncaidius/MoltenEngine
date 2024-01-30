using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal static class SamplerInterop
{
    public static ComparisonFunc ToApi(this ComparisonMode val)
    {
        return (ComparisonFunc)val;
    }

    public static TextureAddressMode ToApi(this SamplerAddressMode mode)
    {
        return (TextureAddressMode)mode;
    }


    /// <summary>Converts a Direct3D11 <see cref="ComparisonFunc"/> to a Molten <see cref="ComparisonMode"/>.</summary>
    /// <param name="val">The value to convert.</param>
    /// <returns></returns>
    public static ComparisonMode FromApi(this ComparisonFunc val)
    {
        return (ComparisonMode)val;
    }

    public static SamplerAddressMode FromApi(this TextureAddressMode mode)
    {
        return (SamplerAddressMode)mode;
    }
}
