using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SDF
{
    public class PseudoDistanceSelector
    {
        SignedDistance minTrueDistance;
        double minNegativePseudoDistance;
        double minPositivePseudoDistance;
        Shape.Edge nearEdge;
        double nearEdgeParam;
        Vector2D p;

        public PseudoDistanceSelector()
        {
            minNegativePseudoDistance = -Math.Abs(minTrueDistance.Distance);
            minPositivePseudoDistance = Math.Abs(minTrueDistance.Distance);
            nearEdge = null;
            nearEdgeParam = 0;
        }

        public void AddEdge(ref EdgeCache cache, Shape.Edge prevEdge, Shape.Edge edge, Shape.Edge nextEdge)
        {
            if (IsEdgeRelevant(cache, edge, p))
            {
                double param;
                SignedDistance distance = edge.SignedDistance(p, out param);
                AddEdgeTrueDistance(edge, distance, param);
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
                    if (GetPseudoDistance(pd, ap, -aDir))
                        AddEdgePseudoDistance(pd = -pd);
                    cache.aPseudoDistance = pd;
                }
                if (bdd > 0)
                {
                    double pd = distance.Distance;
                    if (GetPseudoDistance(pd, bp, bDir))
                        AddEdgePseudoDistance(pd);
                    cache.bPseudoDistance = pd;
                }
                cache.aDomainDistance = add;
                cache.bDomainDistance = bdd;
            }
        }

        public void Reset(ref Vector2D p)
        {
            double delta = SdfGenerator.DISTANCE_DELTA_FACTOR * (p - this.p).Length();
            Reset(delta);
            this.p = p;
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
            return ComputeDistance(p);
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
            minTrueDistance.Distance += MathHelperDP.NonZeroSign(minTrueDistance.Distance) * delta;
            minNegativePseudoDistance = -Math.Abs(minTrueDistance.Distance);
            minPositivePseudoDistance = Math.Abs(minTrueDistance.Distance);
            nearEdge = null;
            nearEdgeParam = 0;
        }

        public bool IsEdgeRelevant(in EdgeCache cache, Shape.Edge edge, in Vector2D p)
        {
            double delta = SdfGenerator.DISTANCE_DELTA_FACTOR * (p - cache.point).Length();
            return (
                cache.absDistance - delta <= Math.Abs(minTrueDistance.Distance) ||
                Math.Abs(cache.aDomainDistance) < delta ||
                Math.Abs(cache.bDomainDistance) < delta ||
                (cache.aDomainDistance > 0 && (cache.aPseudoDistance < 0 ?
                    cache.aPseudoDistance + delta >= minNegativePseudoDistance :
                    cache.aPseudoDistance - delta <= minPositivePseudoDistance
                )) ||
                (cache.bDomainDistance > 0 && (cache.bPseudoDistance < 0 ?
                    cache.bPseudoDistance + delta >= minNegativePseudoDistance :
                    cache.bPseudoDistance - delta <= minPositivePseudoDistance
                ))
            );
        }

        public void AddEdgeTrueDistance(Shape.Edge edge, in SignedDistance distance, double param)
        {
            if (distance < minTrueDistance)
            {
                minTrueDistance = distance;
                nearEdge = edge;
                nearEdgeParam = param;
            }
        }

        public void AddEdgePseudoDistance(double distance)
        {
            if (distance <= 0 && distance > minNegativePseudoDistance)
                minNegativePseudoDistance = distance;
            if (distance >= 0 && distance < minPositivePseudoDistance)
                minPositivePseudoDistance = distance;
        }

        public void Merge(PseudoDistanceSelector other)
        {
            if (other.minTrueDistance < minTrueDistance)
            {
                minTrueDistance = other.minTrueDistance;
                nearEdge = other.nearEdge;
                nearEdgeParam = other.nearEdgeParam;
            }
            if (other.minNegativePseudoDistance > minNegativePseudoDistance)
                minNegativePseudoDistance = other.minNegativePseudoDistance;
            if (other.minPositivePseudoDistance < minPositivePseudoDistance)
                minPositivePseudoDistance = other.minPositivePseudoDistance;
        }

        public double ComputeDistance(in Vector2D p)
        {
            double minDistance = minTrueDistance.Distance < 0 ? minNegativePseudoDistance : minPositivePseudoDistance;
            if (nearEdge != null)
            {
                SignedDistance distance = minTrueDistance;
                nearEdge.DistanceToPseudoDistance(ref distance, p, nearEdgeParam);
                if (Math.Abs(distance.Distance) < Math.Abs(minDistance))
                    minDistance = distance.Distance;
            }
            return minDistance;
        }

        public SignedDistance TrueDistance()
        {
            return minTrueDistance;
        }
    }
}
