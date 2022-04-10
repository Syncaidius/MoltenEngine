using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public struct MultiAndTrueDistance
    {
        public double r, g, b;
        public double a;
    };

    public class MultiAndTrueDistanceSelector : EdgeSelector<MultiAndTrueDistance>
    {
        Vector2D p;
        PseudoDistanceSelectorBase r, g, b;

        public override void reset(in Vector2D p)
        {
            double delta = DISTANCE_DELTA_FACTOR * (p - this.p).Length();
            r.reset(delta);
            g.reset(delta);
            b.reset(delta);
            this.p = p;
        }

        public override void addEdge(ref EdgeCache cache, EdgeSegment prevEdge, EdgeSegment edge, EdgeSegment nextEdge)
        {
            if (
                ((edge.Color & EdgeColor.RED) == EdgeColor.RED && r.isEdgeRelevant(cache, edge, p)) ||
                ((edge.Color & EdgeColor.GREEN) == EdgeColor.GREEN && g.isEdgeRelevant(cache, edge, p)) ||
                ((edge.Color & EdgeColor.BLUE) == EdgeColor.BLUE && b.isEdgeRelevant(cache, edge, p))
            )
            {
                double param;
                SignedDistance distance = edge.signedDistance(p, out param);
                if ((edge.Color & EdgeColor.RED) == EdgeColor.RED)
                    r.addEdgeTrueDistance(edge, distance, param);
                if ((edge.Color & EdgeColor.GREEN) == EdgeColor.GREEN)
                    g.addEdgeTrueDistance(edge, distance, param);
                if ((edge.Color & EdgeColor.BLUE) == EdgeColor.BLUE)
                    b.addEdgeTrueDistance(edge, distance, param);
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
                    if (PseudoDistanceSelectorBase.getPseudoDistance(pd, ap, -aDir))
                    {
                        pd = -pd;
                        if ((edge.Color & EdgeColor.RED) == EdgeColor.RED)
                            r.addEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.GREEN) == EdgeColor.GREEN)
                            g.addEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.BLUE) == EdgeColor.BLUE)
                            b.addEdgePseudoDistance(pd);
                    }
                    cache.aPseudoDistance = pd;
                }
                if (bdd > 0)
                {
                    double pd = distance.Distance;
                    if (PseudoDistanceSelectorBase.getPseudoDistance(pd, bp, bDir))
                    {
                        if ((edge.Color & EdgeColor.RED) == EdgeColor.RED)
                            r.addEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.GREEN) == EdgeColor.GREEN)
                            g.addEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.BLUE) == EdgeColor.BLUE)
                            b.addEdgePseudoDistance(pd);
                    }
                    cache.bPseudoDistance = pd;
                }
                cache.aDomainDistance = add;
                cache.bDomainDistance = bdd;
            }
        }

        public override void merge(EdgeSelector<MultiAndTrueDistance> other)
        {
            MultiAndTrueDistanceSelector mtd = other as MultiAndTrueDistanceSelector;
            r.merge(mtd.r);
            g.merge(mtd.g);
            b.merge(mtd.b);
        }

        /// <summary>
        /// This is a hacky implementation of "base" MultiDistanceSelector.distance() method.
        /// </summary>
        /// <returns></returns>
        private MultiDistance mstDistance()
        {
            MultiDistance multiDistance;
            multiDistance.r = r.computeDistance(p);
            multiDistance.g = g.computeDistance(p);
            multiDistance.b = b.computeDistance(p);
            return multiDistance;
        }

        public override MultiAndTrueDistance distance()
        {
            MultiDistance multiDistance = mstDistance();
            MultiAndTrueDistance mtd;
            mtd.r = multiDistance.r;
            mtd.g = multiDistance.g;
            mtd.b = multiDistance.b;
            mtd.a = trueDistance().Distance;
            return mtd;
        }

        public SignedDistance trueDistance()
        {
            SignedDistance distance = r.trueDistance();
            if (g.trueDistance() < distance)
                distance = g.trueDistance();
            if (b.trueDistance() < distance)
                distance = b.trueDistance();
            return distance;
        }

        public override void initDistance(ref MultiAndTrueDistance distance)
        {
            distance.r = -double.MaxValue;
            distance.g = -double.MaxValue;
            distance.b = -double.MaxValue;
        }

        public override double resolveDistance(MultiAndTrueDistance distance)
        {
            return MsdfMath.median(distance.r, distance.g, distance.b);
        }

        public override float getRefPSD(in MultiAndTrueDistance dist, double invRange)
        {
            return (float)(invRange * dist.r + .5);
        }
    }
}
