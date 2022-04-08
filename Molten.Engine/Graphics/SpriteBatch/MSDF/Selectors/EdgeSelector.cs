using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    public abstract class EdgeSelector
    {
        public const double DISTANCE_DELTA_FACTOR = 1.001;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="DT">Distance type. e.g. double, MultiDistance or MultiAndTrueDistance.</typeparam>
    /// <typeparam name="P">Point type, such as double or Vector2D.</typeparam>
    public abstract class EdgeSelector<DT, P> : EdgeSelector
        where DT : unmanaged
        where P : unmanaged
    {
        public abstract DT distance();

        public abstract void reset(in P p);
    }
}
