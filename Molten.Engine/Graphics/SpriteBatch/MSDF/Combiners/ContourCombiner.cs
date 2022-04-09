using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
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
    public abstract class ContourCombiner<ES, DT, EC> : ContourCombiner
        where ES : EdgeSelector<DT, EC>, new()
        where DT : unmanaged
        where EC : unmanaged
    {
        public abstract void reset(in Vector2D p);

        public abstract ES edgeSelector(int i);

        public abstract DT distance();
    }
}
