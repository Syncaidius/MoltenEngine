using Silk.NET.DXGI;
using Silk.NET.Maths;

namespace Molten.Graphics;

public static class SilkDxgiExtensions
{
    public static Rectangle<int> ToApi(this Rectangle r)
    {
        return new Rectangle<int>(r.X, r.Y, r.Width, r.Height);
    }

    public static Rectangle FromApi(this Rectangle<int> rect)
    {
        return new Rectangle(rect.Origin.X, rect.Origin.Y, rect.Size.X, rect.Size.Y);
    }

    public static Rectangle FromApi(this Box2D<int> r)
    {
        return new Rectangle(r.Min.X, r.Min.Y, r.Size.X, r.Size.Y);
    }

    public static Format ToApi(this GraphicsFormat format)
    {
        return (Format)format;
    }

    public static GraphicsFormat FromApi(this Format format)
    {
        return (GraphicsFormat)format;
    }
}
