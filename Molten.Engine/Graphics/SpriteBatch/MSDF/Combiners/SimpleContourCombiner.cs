using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SpriteBatch.MSDF
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ES">EdgeSelector type.</typeparam>
    /// <typeparam name="DT">Distance type. e.g. double, MultiDistance or MultiAndTrueDistance.</typeparam>
    /// <typeparam name="P">Point type, such as double or Vector2D.</typeparam>
    public class SimpleContourCombiner<ES, DT> : ContourCombiner<ES, DT>
        where ES: EdgeSelector<DT>
        where DT : unmanaged
    {
        ES shapeEdgeSelector;

        public SimpleContourCombiner(MsdfShape shape) { }

        public override void reset(in Vector2D p)
        {
            shapeEdgeSelector.reset(p);
        }

        public override ES edgeSelector(int i)
        {
            return shapeEdgeSelector;
        }

        public override DT distance()
        {
            return shapeEdgeSelector.distance();
        }
    }
}
