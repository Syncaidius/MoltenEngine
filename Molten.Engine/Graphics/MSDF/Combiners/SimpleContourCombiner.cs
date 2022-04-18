using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ES">EdgeSelector type.</typeparam>
    /// <typeparam name="DT">Distance type. e.g. double, MultiDistance or MultiAndTrueDistance.</typeparam>
    public class SimpleContourCombiner<ES, DT> : ContourCombiner<ES, DT>
        where ES: EdgeSelector<DT>, new()
        where DT : unmanaged
    {
        ES shapeEdgeSelector = new ES();

        public SimpleContourCombiner(MsdfShape shape) { }

        public override void Reset(ref Vector2D p)
        {
            shapeEdgeSelector.Reset(ref p);
        }

        public override ES EdgeSelector(int i)
        {
            return shapeEdgeSelector;
        }

        public override DT Distance()
        {
            return shapeEdgeSelector.Distance();
        }
    }
}
