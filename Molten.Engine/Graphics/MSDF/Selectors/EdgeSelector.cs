using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public abstract class EdgeSelector
    {
        public const double DISTANCE_DELTA_FACTOR = 1.001;

        public abstract void reset(in Vector2D p);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="DT">Distance type. e.g. double, MultiDistance or MultiAndTrueDistance.</typeparam>
    /// <typeparam name="EC">EdgeCache type.</typeparam>
    public abstract class EdgeSelector<DT> : EdgeSelector
        where DT : unmanaged
    {
        public abstract DT distance();

        public abstract void addEdge(ref EdgeCache cache, EdgeSegment prevEdge, EdgeSegment edge, EdgeSegment nextEdge);

        public abstract void merge(EdgeSelector<DT> other);

        public abstract double resolveDistance(DT distance);

        public abstract void initDistance(ref DT distance);

        public abstract float getRefPSD(in DT dist, double invRange);
    }
}
