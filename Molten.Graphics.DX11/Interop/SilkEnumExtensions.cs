using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11;

internal static class SilkEnumExtensions
{
    public static bool HasFlag(this FormatSupport value, FormatSupport flag)
    {
        return (value & flag) == flag;
    }
}
