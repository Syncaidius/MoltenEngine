using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public abstract class ContourCombiner
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ES">EdgeSelector type.</typeparam>
    /// <typeparam name="DT">Distance type. e.g. double, MultiDistance or MultiAndTrueDistance.</typeparam>
    /// <typeparam name="EC">EdgeCache type.</typeparam>
    public abstract class ContourCombiner<ES, DT> : ContourCombiner
        where ES : EdgeSelector<DT>, new()
        where DT : unmanaged
    {
        public abstract void Reset(ref Vector2D p);

        public abstract ES EdgeSelector(int i);

        public abstract DT Distance();
    }
}
