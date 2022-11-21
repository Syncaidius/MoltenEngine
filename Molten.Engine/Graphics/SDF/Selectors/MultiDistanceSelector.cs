using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.DoublePrecision;

namespace Molten.Graphics.SDF
{
    public class MultiDistanceSelector
    {
        Vector2D P;
        PseudoDistanceSelector R = new PseudoDistanceSelector();
        PseudoDistanceSelector G = new PseudoDistanceSelector();
        PseudoDistanceSelector B = new PseudoDistanceSelector();

        public void Reset(ref Vector2D p)
        {
            double delta = SdfGenerator.DISTANCE_DELTA_FACTOR * (p - P).Length();
            R.Reset(delta);
            G.Reset(delta);
            B.Reset(delta);
            P = p;
        }

        public void AddEdge(ref EdgeCache cache, Shape.Edge prevEdge, Shape.Edge edge, Shape.Edge nextEdge)
        {
            if (
                ((edge.Color & EdgeColor.Red) == EdgeColor.Red && R.IsEdgeRelevant(cache, edge, P)) ||
                ((edge.Color & EdgeColor.Green) == EdgeColor.Green && G.IsEdgeRelevant(cache, edge, P)) ||
                ((edge.Color & EdgeColor.Blue) == EdgeColor.Blue && B.IsEdgeRelevant(cache, edge, P))
            )
            {
                double param;
                SignedDistance distance = edge.SignedDistance(P, out param);
                if ((edge.Color & EdgeColor.Red) == EdgeColor.Red)
                    R.AddEdgeTrueDistance(edge, distance, param);
                if ((edge.Color & EdgeColor.Green) == EdgeColor.Green)
                    G.AddEdgeTrueDistance(edge, distance, param);
                if ((edge.Color & EdgeColor.Blue) == EdgeColor.Blue)
                    B.AddEdgeTrueDistance(edge, distance, param);
                cache.Point = P;
                cache.AbsDistance = Math.Abs(distance.Distance);

                Vector2D ap = P - edge.Point(0);
                Vector2D bp = P - edge.Point(1);
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
                    cache.APseudoDistance = pd;
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
                    cache.BPseudoDistance = pd;
                }
                cache.ADomainDistance = add;
                cache.BDomainDistance = bdd;
            }
        }

        public void Merge(MultiDistanceSelector other)
        {
            MultiDistanceSelector md = other as MultiDistanceSelector;
            R.Merge(md.R);
            G.Merge(md.G);
            B.Merge(md.B);
        }

        public Color3D Distance()
        {
            Color3D multiDistance;
            multiDistance.R = R.ComputeDistance(P);
            multiDistance.G = G.ComputeDistance(P);
            multiDistance.B = B.ComputeDistance(P);
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

        public void InitDistance(ref Color3D distance)
        {
            distance.R = -double.MaxValue;
            distance.G = -double.MaxValue;
            distance.B = -double.MaxValue;
        }

        public double ResolveDistance(Color3D distance)
        {
            return MathHelper.Median(distance.R, distance.G, distance.B);
        }

        public float GetRefPSD(ref Color3D dist, double invRange)
        {
            return (float)(invRange * dist.R + .5);
        }
    }
}
