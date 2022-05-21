using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SDF
{
    public struct MultiDistance
    {
        public double r, g, b;
    };

    public class MultiDistanceSelector
    {
        Vector2D p;
        PseudoDistanceSelector R = new PseudoDistanceSelector();
        PseudoDistanceSelector G = new PseudoDistanceSelector();
        PseudoDistanceSelector B = new PseudoDistanceSelector();

        public void Reset(ref Vector2D p)
        {
            double delta = SdfGenerator.DISTANCE_DELTA_FACTOR * (p - this.p).Length();
            R.Reset(delta);
            G.Reset(delta);
            B.Reset(delta);
            this.p = p;
        }

        public void AddEdge(ref EdgeCache cache, Shape.Edge prevEdge, Shape.Edge edge, Shape.Edge nextEdge)
        {
            if (
                ((edge.Color & EdgeColor.Red) == EdgeColor.Red && R.IsEdgeRelevant(cache, edge, p)) ||
                ((edge.Color & EdgeColor.Green) == EdgeColor.Green && G.IsEdgeRelevant(cache, edge, p)) ||
                ((edge.Color & EdgeColor.Blue) == EdgeColor.Blue && B.IsEdgeRelevant(cache, edge, p))
            )
            {
                double param;
                SignedDistance distance = edge.SignedDistance(p, out param);
                if ((edge.Color & EdgeColor.Red) == EdgeColor.Red)
                    R.AddEdgeTrueDistance(edge, distance, param);
                if ((edge.Color & EdgeColor.Green) == EdgeColor.Green)
                    G.AddEdgeTrueDistance(edge, distance, param);
                if ((edge.Color & EdgeColor.Blue) == EdgeColor.Blue)
                    B.AddEdgeTrueDistance(edge, distance, param);
                cache.point = p;
                cache.absDistance = Math.Abs(distance.Distance);

                Vector2D ap = p - edge.Point(0);
                Vector2D bp = p - edge.Point(1);
                Vector2D aDir = edge.GetDirection(0).GetNormalized(true);
                Vector2D bDir = edge.GetDirection(1).GetNormalized(true);
                Vector2D prevDir = prevEdge.GetDirection(1).GetNormalized(true);
                Vector2D nextDir = nextEdge.GetDirection(0).GetNormalized(true);
                double add = Vector2D.Dot(ap, (prevDir + aDir).GetNormalized(true));
                double bdd = -Vector2D.Dot(bp, (bDir + nextDir).GetNormalized(true));
                if (add > 0)
                {
                    double pd = distance.Distance;
                    if (PseudoDistanceSelector.GetPseudoDistance(pd, ap, -aDir))
                    {
                        pd = -pd;
                        if ((edge.Color & EdgeColor.Red) == EdgeColor.Red)
                            R.AddEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.Green) == EdgeColor.Green)
                            G.AddEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.Blue) == EdgeColor.Blue)
                            B.AddEdgePseudoDistance(pd);
                    }
                    cache.aPseudoDistance = pd;
                }
                if (bdd > 0)
                {
                    double pd = distance.Distance;
                    if (PseudoDistanceSelector.GetPseudoDistance(pd, bp, bDir))
                    {
                        if ((edge.Color & EdgeColor.Red) == EdgeColor.Red)
                            R.AddEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.Green) == EdgeColor.Green)
                            G.AddEdgePseudoDistance(pd);
                        if ((edge.Color & EdgeColor.Blue) == EdgeColor.Blue)
                            B.AddEdgePseudoDistance(pd);
                    }
                    cache.bPseudoDistance = pd;
                }
                cache.aDomainDistance = add;
                cache.bDomainDistance = bdd;
            }
        }

        public void Merge(MultiDistanceSelector other)
        {
            MultiDistanceSelector md = other as MultiDistanceSelector;
            R.Merge(md.R);
            G.Merge(md.G);
            B.Merge(md.B);
        }

        public MultiDistance Distance()
        {
            MultiDistance multiDistance;
            multiDistance.r = R.ComputeDistance(p);
            multiDistance.g = G.ComputeDistance(p);
            multiDistance.b = B.ComputeDistance(p);
            return multiDistance;
        }

        public SignedDistance TrueDistance()
        {
            SignedDistance distance = R.TrueDistance();
            if (G.TrueDistance() < distance)
                distance = G.TrueDistance();
            if (B.TrueDistance() < distance)
                distance = B.TrueDistance();
            return distance;
        }

        public void InitDistance(ref MultiDistance distance)
        {
            distance.r = -double.MaxValue;
            distance.g = -double.MaxValue;
            distance.b = -double.MaxValue;
        }

        public double ResolveDistance(MultiDistance distance)
        {
            return MathHelperDP.Median(distance.r, distance.g, distance.b);
        }

        public float GetRefPSD(ref MultiDistance dist, double invRange)
        {
            return (float)(invRange * dist.r + .5);
        }
    }
}
