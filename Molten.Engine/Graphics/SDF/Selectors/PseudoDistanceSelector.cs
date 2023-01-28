using Molten.DoublePrecision;

namespace Molten.Graphics.SDF
{
    public class PseudoDistanceSelector
    {
        SignedDistance MinTrueDistance;
        double MinNegativePseudoDistance;
        double MinPositivePseudoDistance;
        Shape.Edge NearEdge;
        double NearEdgeParam;
        Vector2D P;

        public PseudoDistanceSelector()
        {
            MinNegativePseudoDistance = -Math.Abs(MinTrueDistance.Distance);
            MinPositivePseudoDistance = Math.Abs(MinTrueDistance.Distance);
            NearEdge = null;
            NearEdgeParam = 0;
        }

        public void AddEdge(ref EdgeCache cache, Shape.Edge prevEdge, Shape.Edge edge, Shape.Edge nextEdge)
        {
            if (IsEdgeRelevant(cache, edge, P))
            {
                double param;
                SignedDistance distance = edge.SignedDistance(P, out param);
                AddEdgeTrueDistance(edge, distance, param);
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
                    if (GetPseudoDistance(pd, ap, -aDir))
                        AddEdgePseudoDistance(pd = -pd);
                    cache.APseudoDistance = pd;
                }
                if (bdd > 0)
                {
                    double pd = distance.Distance;
                    if (GetPseudoDistance(pd, bp, bDir))
                        AddEdgePseudoDistance(pd);
                    cache.BPseudoDistance = pd;
                }
                cache.ADomainDistance = add;
                cache.BDomainDistance = bdd;
            }
        }

        public void Reset(ref Vector2D p)
        {
            double delta = SdfGenerator.DISTANCE_DELTA_FACTOR * (p - this.P).Length();
            Reset(delta);
            this.P = p;
        }

        public void InitDistance(ref double distance)
        {
            distance = -double.MaxValue;
        }

        public double ResolveDistance(double distance)
        {
            return distance;
        }

        public float GetRefPSD(ref double dist, double invRange)
        {
            return (float)(invRange * dist + .5);
        }

        public double Distance()
        {
            return ComputeDistance(P);
        }

        public static bool GetPseudoDistance(double distance, in Vector2D ep, in Vector2D edgeDir)
        {
            double ts = Vector2D.Dot(ep, edgeDir);
            if (ts > 0)
            {
                double pseudoDistance = Vector2D.Cross(ep, edgeDir);
                if (Math.Abs(pseudoDistance) < Math.Abs(distance))
                {
                    distance = pseudoDistance;
                    return true;
                }
            }
            return false;
        }

        public void Reset(in double delta)
        {
            MinTrueDistance.Distance += MathHelper.NonZeroSign(MinTrueDistance.Distance) * delta;
            MinNegativePseudoDistance = -Math.Abs(MinTrueDistance.Distance);
            MinPositivePseudoDistance = Math.Abs(MinTrueDistance.Distance);
            NearEdge = null;
            NearEdgeParam = 0;
        }

        public bool IsEdgeRelevant(in EdgeCache cache, Shape.Edge edge, in Vector2D p)
        {
            double delta = SdfGenerator.DISTANCE_DELTA_FACTOR * (p - cache.Point).Length();
            return (
                cache.AbsDistance - delta <= Math.Abs(MinTrueDistance.Distance) ||
                Math.Abs(cache.ADomainDistance) < delta ||
                Math.Abs(cache.BDomainDistance) < delta ||
                (cache.ADomainDistance > 0 && (cache.APseudoDistance < 0 ?
                    cache.APseudoDistance + delta >= MinNegativePseudoDistance :
                    cache.APseudoDistance - delta <= MinPositivePseudoDistance
                )) ||
                (cache.BDomainDistance > 0 && (cache.BPseudoDistance < 0 ?
                    cache.BPseudoDistance + delta >= MinNegativePseudoDistance :
                    cache.BPseudoDistance - delta <= MinPositivePseudoDistance
                ))
            );
        }

        public void AddEdgeTrueDistance(Shape.Edge edge, in SignedDistance distance, double param)
        {
            if (distance < MinTrueDistance)
            {
                MinTrueDistance = distance;
                NearEdge = edge;
                NearEdgeParam = param;
            }
        }

        public void AddEdgePseudoDistance(double distance)
        {
            if (distance <= 0 && distance > MinNegativePseudoDistance)
                MinNegativePseudoDistance = distance;
            if (distance >= 0 && distance < MinPositivePseudoDistance)
                MinPositivePseudoDistance = distance;
        }

        public void Merge(PseudoDistanceSelector other)
        {
            if (other.MinTrueDistance < MinTrueDistance)
            {
                MinTrueDistance = other.MinTrueDistance;
                NearEdge = other.NearEdge;
                NearEdgeParam = other.NearEdgeParam;
            }
            if (other.MinNegativePseudoDistance > MinNegativePseudoDistance)
                MinNegativePseudoDistance = other.MinNegativePseudoDistance;
            if (other.MinPositivePseudoDistance < MinPositivePseudoDistance)
                MinPositivePseudoDistance = other.MinPositivePseudoDistance;
        }

        public double ComputeDistance(in Vector2D p)
        {
            double minDistance = MinTrueDistance.Distance < 0 ? MinNegativePseudoDistance : MinPositivePseudoDistance;
            if (NearEdge != null)
            {
                SignedDistance distance = MinTrueDistance;
                NearEdge.DistanceToPseudoDistance(ref distance, p, NearEdgeParam);
                if (Math.Abs(distance.Distance) < Math.Abs(minDistance))
                    minDistance = distance.Distance;
            }
            return minDistance;
        }

        public SignedDistance TrueDistance()
        {
            return MinTrueDistance;
        }
    }
}
