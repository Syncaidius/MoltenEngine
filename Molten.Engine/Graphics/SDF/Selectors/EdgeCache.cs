using Molten.DoublePrecision;

namespace Molten.Graphics.SDF;

public struct EdgeCache
{
    public Vector2D Point;
    public double AbsDistance;
    public double ADomainDistance, BDomainDistance;
    public double APseudoDistance, BPseudoDistance;

    public EdgeCache(Vector2D p, double absDist)
    {
        Point = p;
        AbsDistance = absDist;
        ADomainDistance = 0;
        BDomainDistance = 0;
        APseudoDistance = 0;
        BPseudoDistance = 0;
    }

    public EdgeCache(Vector2D p, double absDist, double aDomainDist, double bDomainDist, double aPseudoDist, double bPseudoDist)
    {
        Point = p;
        AbsDistance = absDist;
        ADomainDistance = aDomainDist;
        BDomainDistance = bDomainDist;
        APseudoDistance = aPseudoDist;
        BPseudoDistance = bPseudoDist;
    }
}
