//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017, WinterDev (C# port) from https://github.com/LayoutFarm/Typography/tree/master/Typography.MsdfGen

/// Edge color specifies which color channels an edge belongs to.
namespace Molten.Render.Msdf
{
    public enum EdgeColor
    {
        BLACK = 0,
        RED = 1,
        GREEN = 2,
        YELLOW = 3,
        BLUE = 4,
        MAGENTA = 5,
        CYAN = 6,
        WHITE = 7
    }
}