using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public struct EdgeCache
    {
        public Vector2D point;
        public double absDistance;
        public double aDomainDistance, bDomainDistance;
        public double aPseudoDistance, bPseudoDistance;

        public EdgeCache(Vector2D p, double absDist)
        {
            point = p;
            absDistance = absDist;
            aDomainDistance = 0;
            bDomainDistance = 0;
            aPseudoDistance = 0;
            bPseudoDistance = 0;
        }

        public EdgeCache(Vector2D p, double absDist, double aDomainDist, double bDomainDist, double aPseudoDist, double bPseudoDist)
        {
            point = p;
            absDistance = absDist;
            aDomainDistance = aDomainDist;
            bDomainDistance = bDomainDist;
            aPseudoDistance = aPseudoDist;
            bPseudoDistance = bPseudoDist;
        }
    }
}
