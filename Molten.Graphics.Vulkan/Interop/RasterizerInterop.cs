using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal static class RasterizerInterop
{
    public static PolygonMode ToApi(this RasterizerFillingMode mode)
    {
        switch (mode)
        {
            default:
            case RasterizerFillingMode.Solid:
                return PolygonMode.Fill;

            case RasterizerFillingMode.Wireframe:
                return PolygonMode.Line;

            case RasterizerFillingMode.Point:
                return PolygonMode.Point;
        }
    }

    public static CullModeFlags ToApi(this RasterizerCullingMode mode)
    {
        switch(mode){
            default:
            case RasterizerCullingMode.None:
                return CullModeFlags.None;

            case RasterizerCullingMode.Back:
                return CullModeFlags.BackBit;

            case RasterizerCullingMode.Front:
                return CullModeFlags.FrontBit;

            case RasterizerCullingMode.All:
                return CullModeFlags.BackBit | CullModeFlags.FrontBit;
        }
    }
}
