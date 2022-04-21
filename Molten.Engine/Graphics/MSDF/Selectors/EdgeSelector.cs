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

        public abstract void Reset(ref Vector2D p);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="DT">Distance type. e.g. double, MultiDistance or MultiAndTrueDistance.</typeparam>
    /// <typeparam name="EC">EdgeCache type.</typeparam>
    public abstract class EdgeSelector<DT> : EdgeSelector
        where DT : unmanaged
    {
        public abstract DT Distance();

        public abstract void AddEdge(ref EdgeCache cache, ContourShape.Edge prevEdge, ContourShape.Edge edge, ContourShape.Edge nextEdge);

        public abstract void Merge(EdgeSelector<DT> other);

        public abstract double ResolveDistance(DT distance);

        public abstract void InitDistance(ref DT distance);

        public abstract float GetRefPSD(ref DT dist, double invRange);
    }
}
