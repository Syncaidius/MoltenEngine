using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public class TrueDistanceSelector : EdgeSelector<double>
    {
        Vector2D p;
        SignedDistance minDistance;

        public override void Reset(ref Vector2D p)
        {
            double delta = DISTANCE_DELTA_FACTOR * (p - this.p).Length();
            minDistance.Distance += MathHelperDP.NonZeroSign(minDistance.Distance) * delta;
            this.p = p;
        }

        public override unsafe void AddEdge(ref EdgeCache cache, ContourShape.Edge prevEdge, ContourShape.Edge edge, ContourShape.Edge nextEdge)
        {
            double delta = DISTANCE_DELTA_FACTOR * (p - cache.point).Length();
            if (cache.absDistance - delta <= Math.Abs(minDistance.Distance))
            {
                double dummy;
                SignedDistance distance = edge.SignedDistance(p, out dummy);
                if (distance < minDistance)
                    minDistance = distance;
                cache.point = p;
                cache.absDistance = Math.Abs(distance.Distance);
            }
        }

        public override void Merge(EdgeSelector<double> other)
        {
            TrueDistanceSelector td = other as TrueDistanceSelector;
            if (td.minDistance < minDistance)
                minDistance = td.minDistance;
        }

        public override double Distance()
        {
            return minDistance.Distance;
        }

        public override void InitDistance(ref double distance)
        {
            distance = -double.MaxValue;
        }

        public override double ResolveDistance(double distance)
        {
            return distance;
        }

        public override float GetRefPSD(ref double dist, double invRange)
        {
            return (float)(invRange * dist + .5);
        }
    }
}
