using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF.Selectors
{
    internal class PseudoDistanceSelector : PseudoDistanceSelectorBase
    {
        Vector2D p;

        public void reset(in Vector2D p)
        {
            double delta = DISTANCE_DELTA_FACTOR * (p - this.p).Length();
            reset(delta);
            this.p = p;
        }

        public void addEdge(ref EdgeCache cache, EdgeSegment prevEdge, EdgeSegment edge, EdgeSegment nextEdge)
        {
            if (isEdgeRelevant(cache, edge, p))
            {
                double param;
                SignedDistance distance = edge.signedDistance(p, out param);
                addEdgeTrueDistance(edge, distance, param);
                cache.point = p;
                cache.absDistance = Math.Abs(distance.Distance);

                Vector2D ap = p - edge.point(0);
                Vector2D bp = p - edge.point(1);
                Vector2D aDir = edge.direction(0).GetNormalized(true);
                Vector2D bDir = edge.direction(1).GetNormalized(true);
                Vector2D prevDir = prevEdge.direction(1).GetNormalized(true);
                Vector2D nextDir = nextEdge.direction(0).GetNormalized(true);
                double add = Vector2D.Dot(ap, (prevDir + aDir).GetNormalized(true));
                double bdd = -Vector2D.Dot(bp, (bDir + nextDir).GetNormalized(true));
                if (add > 0)
                {
                    double pd = distance.Distance;
                    if (getPseudoDistance(pd, ap, -aDir))
                        addEdgePseudoDistance(pd = -pd);
                    cache.aPseudoDistance = pd;
                }
                if (bdd > 0)
                {
                    double pd = distance.Distance;
                    if (getPseudoDistance(pd, bp, bDir))
                        addEdgePseudoDistance(pd);
                    cache.bPseudoDistance = pd;
                }
                cache.aDomainDistance = add;
                cache.bDomainDistance = bdd;
            }
        }

        public override double distance()
        {
            return computeDistance(p);
        }
    }
}
