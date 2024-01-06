namespace Molten.Graphics;

public enum GraphicsMapType
{
    Read = 0,

    Write = 1,

    Discard = 2,
}

public static class GraphicsMapTypeExtensions
{
    public static bool Has(this GraphicsMapType self, GraphicsMapType flag)
    {
        return (self & flag) == flag;
    }
}
