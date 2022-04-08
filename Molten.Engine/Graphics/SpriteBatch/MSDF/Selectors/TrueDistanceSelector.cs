using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    public class TrueDistanceSelector : EdgeSelector<double>
    {
        Vector2D p;
        SignedDistance minDistance;

        public struct EdgeCache
        {
            public Vector2D point;
            public double absDistance;

            public EdgeCache(Vector2D p, double absDist)
            {
                point = p;
                absDistance = absDist;
            }
        }

        public override void reset(in Vector2D p)
        {
            double delta = DISTANCE_DELTA_FACTOR * (p - this.p).Length();
            minDistance.Distance += MsdfMath.nonZeroSign(minDistance.Distance) * delta;
            this.p = p;
        }

        public unsafe void addEdge(ref EdgeCache cache, EdgeSegment prevEdge, EdgeSegment edge, EdgeSegment nextEdge)
        {
            double delta = DISTANCE_DELTA_FACTOR * (p - cache.point).Length();
            if (cache.absDistance - delta <= Math.Abs(minDistance.Distance))
            {
                double dummy;
                SignedDistance distance = edge.signedDistance(p, out dummy);
                if (distance < minDistance)
                    minDistance = distance;
                cache.point = p;
                cache.absDistance = Math.Abs(distance.Distance);
            }
        }

        public void merge(TrueDistanceSelector other)
        {
            if (other.minDistance < minDistance)
                minDistance = other.minDistance;
        }

        public override double distance()
        {
            return new Vector2D(minDistance.Distance);
        }
    }
}
