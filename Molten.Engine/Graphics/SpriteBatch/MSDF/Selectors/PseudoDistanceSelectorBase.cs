using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    public abstract class PseudoDistanceSelectorBase : EdgeSelector<double, PseudoDistanceSelectorBase.EdgeCache>
    {
        SignedDistance minTrueDistance;
        double minNegativePseudoDistance;
        double minPositivePseudoDistance;
        EdgeSegment nearEdge;
        double nearEdgeParam;

        public struct EdgeCache
        {
            public Vector2D point;
            public double absDistance;
            public double aDomainDistance, bDomainDistance;
            public double aPseudoDistance, bPseudoDistance;
        }

        public PseudoDistanceSelectorBase()
        {
            minNegativePseudoDistance = -Math.Abs(minTrueDistance.Distance);
            minPositivePseudoDistance = Math.Abs(minTrueDistance.Distance);
            nearEdge = null;
            nearEdgeParam = 0;
        }

        public static bool getPseudoDistance(double distance, in Vector2D ep, in Vector2D edgeDir)
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

        public void reset(in double delta)
        {
            minTrueDistance.Distance += MsdfMath.nonZeroSign(minTrueDistance.Distance) * delta;
            minNegativePseudoDistance = -Math.Abs(minTrueDistance.Distance);
            minPositivePseudoDistance = Math.Abs(minTrueDistance.Distance);
            nearEdge = null;
            nearEdgeParam = 0;
        }

        public bool isEdgeRelevant(in EdgeCache cache, EdgeSegment edge, in Vector2D p)
        {
            double delta = DISTANCE_DELTA_FACTOR * (p - cache.point).Length();
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

        public void addEdgeTrueDistance(EdgeSegment edge, in SignedDistance distance, double param)
        {
            if (distance < minTrueDistance)
            {
                minTrueDistance = distance;
                nearEdge = edge;
                nearEdgeParam = param;
            }
        }

        public void addEdgePseudoDistance(double distance)
        {
            if (distance <= 0 && distance > minNegativePseudoDistance)
                minNegativePseudoDistance = distance;
            if (distance >= 0 && distance < minPositivePseudoDistance)
                minPositivePseudoDistance = distance;
        }

        public override void merge(EdgeSelector<double, EdgeCache> other)
        {
            PseudoDistanceSelectorBase pd = other as PseudoDistanceSelectorBase;

            if (pd.minTrueDistance < minTrueDistance)
            {
                minTrueDistance = pd.minTrueDistance;
                nearEdge = pd.nearEdge;
                nearEdgeParam = pd.nearEdgeParam;
            }
            if (pd.minNegativePseudoDistance > minNegativePseudoDistance)
                minNegativePseudoDistance = pd.minNegativePseudoDistance;
            if (pd.minPositivePseudoDistance < minPositivePseudoDistance)
                minPositivePseudoDistance = pd.minPositivePseudoDistance;
        }

        public double computeDistance(in Vector2D p)
        {
            double minDistance = minTrueDistance.Distance < 0 ? minNegativePseudoDistance : minPositivePseudoDistance;
            if (nearEdge != null)
            {
                SignedDistance distance = minTrueDistance;
                nearEdge.distanceToPseudoDistance(ref distance, p, nearEdgeParam);
                if (Math.Abs(distance.Distance) < Math.Abs(minDistance))
                    minDistance = distance.Distance;
            }
            return minDistance;
        }

        public SignedDistance trueDistance()
        {
            return minTrueDistance;
        }
    }
}
