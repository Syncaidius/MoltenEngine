namespace Molten.Graphics;

public enum GpuMapType
{
    Read = 0,

    Write = 1,

    Discard = 2,
}

public static class GraphicsMapTypeExtensions
{
    public static bool Has(this GpuMapType self, GpuMapType flag)
    {
        return (self & flag) == flag;
    }
}
