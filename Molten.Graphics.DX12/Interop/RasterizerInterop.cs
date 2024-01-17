using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal static class RasterizerInterop
{
    public static FillMode ToApi(this RasterizerFillingMode mode)
    {
        switch (mode)
        {
            case RasterizerFillingMode.Solid:
                return FillMode.Solid;

            case RasterizerFillingMode.Wireframe:
                return FillMode.Wireframe;

            default:
                throw new NotSupportedException($"DirectX 11 mode does not support a RasterizerFillingMode of '{mode}'");
        }
    }

    public static CullMode ToApi(this RasterizerCullingMode mode)
    {
        switch (mode)
        {
            case RasterizerCullingMode.None:
                return CullMode.None;

            case RasterizerCullingMode.Back:
                return CullMode.Back;

            case RasterizerCullingMode.Front:
                return CullMode.Front;

            default:
                throw new NotSupportedException($"DirectX 11 mode does not support a RasterizerFillingMode of '{mode}'");
        }
    }
}
