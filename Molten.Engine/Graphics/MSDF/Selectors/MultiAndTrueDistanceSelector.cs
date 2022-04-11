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
        PseudoDistanceSelector r, g, b;

        public override void Reset(ref Vector2D p)
        {
            double delta = DISTANCE_DELTA_FACTOR * (p - this.p).Length();
            r.Reset(delta);
            g.Reset(delta);
            b.Reset(delta);
            this.p = p;
        }

        public override void AddEdge(ref EdgeCache cache, EdgeSegment prevEdge, EdgeSegment edge, EdgeSegment nextEdge)
        {
            if (
                ((edge.Color & EdgeColor.Red) == EdgeColor.Red && r.IsEdgeRelevant(cache, edge, p)) ||
                ((edge.Color & EdgeColor.Green) == EdgeColor.Green && g.IsEdgeRelevant(cache, edge, p)) ||
                ((edge.Color & EdgeColor.Blue) == EdgeColor.Blue && b.IsEdgeRelevant(cache, edge, p))
            )
            {
                double param;
                SignedDistance distance = edge.SignedDistance(p, out param);
                if ((edge.Color & EdgeColor.Red) == EdgeColor.Red)
                    r.AddEdgeTrueDistance(edge, distance, param);
                if ((edge.Color & EdgeColor.Green) == EdgeColor.Green)
                    g.AddEdgeTrueDistance(edge, distance, param);
                if ((edge.Color & EdgeColor.Blue) == EdgeColor.Blue)
                    b.AddEdgeTrueDistance(edge, distance, param);
                cache.point = p;
                cache.absDistance = Math.Abs(distance.Distance);

                Vector2D ap = p - edge.Point(0);
                Vector2D bp = p - edge.Point(1);
                Vector2D aDir = edge.Direction(0).GetNormalized(true);
                Vector2D bDir = edge.Direction(1).GetNormalized(true);
                Vector2D prevDir = prevEdge.Direction(1).GetNormalized(true);
                Vector2D nextDir = nextEdge.Direction(0).GetNormalized(true);
                double add = Vector2D.Dot(ap, (prevDir + aDir).GetNormalized(true));
                double bdd = -Vector2D.Dot(bp, (bDir + nextDir).GetNormalized(true));
                if (add > 0)
                {
                    double pd = distance.Distance;
                    if (PseudoDistanceSelector.GetPseudoDistance(pd, ap, -aDir))
                    {
                        pd = -pd;
                        if ((edge.Color & EdgeColor.Red) == EdgeColor.Red)
                            r.AddEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.Green) == EdgeColor.Green)
                            g.AddEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.Blue) == EdgeColor.Blue)
                            b.AddEdgePseudoDistance(pd);
                    }
                    cache.aPseudoDistance = pd;
                }
                if (bdd > 0)
                {
                    double pd = distance.Distance;
                    if (PseudoDistanceSelector.GetPseudoDistance(pd, bp, bDir))
                    {
                        if ((edge.Color & EdgeColor.Red) == EdgeColor.Red)
                            r.AddEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.Green) == EdgeColor.Green)
                            g.AddEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.Blue) == EdgeColor.Blue)
                            b.AddEdgePseudoDistance(pd);
                    }
                    cache.bPseudoDistance = pd;
                }
                cache.aDomainDistance = add;
                cache.bDomainDistance = bdd;
            }
        }

        public override void Merge(EdgeSelector<MultiAndTrueDistance> other)
        {
            MultiAndTrueDistanceSelector mtd = other as MultiAndTrueDistanceSelector;
            r.Merge(mtd.r);
            g.Merge(mtd.g);
            b.Merge(mtd.b);
        }

        /// <summary>
        /// This is a hacky implementation of "base" MultiDistanceSelector.distance() method.
        /// </summary>
        /// <returns></returns>
        private MultiDistance MstDistance()
        {
            MultiDistance multiDistance;
            multiDistance.r = r.ComputeDistance(p);
            multiDistance.g = g.ComputeDistance(p);
            multiDistance.b = b.ComputeDistance(p);
            return multiDistance;
        }

        public override MultiAndTrueDistance Distance()
        {
            MultiDistance multiDistance = MstDistance();
            MultiAndTrueDistance mtd;
            mtd.r = multiDistance.r;
            mtd.g = multiDistance.g;
            mtd.b = multiDistance.b;
            mtd.a = TrueDistance().Distance;
            return mtd;
        }

        public SignedDistance TrueDistance()
        {
            SignedDistance distance = r.TrueDistance();
            if (g.TrueDistance() < distance)
                distance = g.TrueDistance();
            if (b.TrueDistance() < distance)
                distance = b.TrueDistance();
            return distance;
        }

        public override void InitDistance(ref MultiAndTrueDistance distance)
        {
            distance.r = -double.MaxValue;
            distance.g = -double.MaxValue;
            distance.b = -double.MaxValue;
        }

        public override double ResolveDistance(MultiAndTrueDistance distance)
        {
            return MsdfMath.Median(distance.r, distance.g, distance.b);
        }

        public override float GetRefPSD(ref MultiAndTrueDistance dist, double invRange)
        {
            return (float)(invRange * dist.r + .5);
        }
    }
}
