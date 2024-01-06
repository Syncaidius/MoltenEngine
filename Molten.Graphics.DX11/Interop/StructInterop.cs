using Silk.NET.Direct3D11;

namespace Molten.Graphics.DX11;

internal static class StructInterop
{
    public unsafe static Box ToApi(this ResourceRegion region)
    {
        return *(Box*)&region;
    }
}
