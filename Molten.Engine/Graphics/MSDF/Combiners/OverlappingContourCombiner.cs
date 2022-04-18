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
    /// <typeparam name="EC">Edge cache type.</typeparam>
    public class OverlappingContourCombiner<ES, DT> : ContourCombiner<ES, DT>
         where ES : EdgeSelector<DT>, new()
        where DT : unmanaged
    {
        Vector2D p;
        List<int> windings;
        List<ES> edgeSelectors;

        public OverlappingContourCombiner(MsdfShape shape)
        {
            windings = new List<int>(shape.Contours.Count);
            foreach (Contour contour in shape.Contours)
                windings.Add(contour.Winding());

            edgeSelectors = new List<ES>(shape.Contours.Count);
            for (int i = 0; i < shape.Contours.Count; i++)
                edgeSelectors.Add(new ES());
        }

        public override void Reset(ref Vector2D p)
        {
            this.p = p;
            foreach (EdgeSelector<DT> contourEdgeSelector in edgeSelectors)
                contourEdgeSelector.Reset(ref p);
        }

        public override ES EdgeSelector(int i)
        {
            return edgeSelectors[i];
        }

        public override DT Distance()
        {
            int contourCount = edgeSelectors.Count;
            ES shapeEdgeSelector = new ES();
            ES innerEdgeSelector = new ES();
            ES outerEdgeSelector = new ES();

            shapeEdgeSelector.Reset(ref p);
            innerEdgeSelector.Reset(ref p);
            outerEdgeSelector.Reset(ref p);

            for (int i = 0; i < contourCount; ++i)
            {
                DT edgeDistance = edgeSelectors[i].Distance();
                shapeEdgeSelector.Merge(edgeSelectors[i]);
                if (windings[i] > 0 && edgeSelectors[i].ResolveDistance(edgeDistance) >= 0)
                    innerEdgeSelector.Merge(edgeSelectors[i]);
                if (windings[i] < 0 && edgeSelectors[i].ResolveDistance(edgeDistance) <= 0)
                    outerEdgeSelector.Merge(edgeSelectors[i]);
            }

            DT shapeDistance = shapeEdgeSelector.Distance();
            DT innerDistance = innerEdgeSelector.Distance();
            DT outerDistance = outerEdgeSelector.Distance();

            double innerScalarDistance = shapeEdgeSelector.ResolveDistance(innerDistance);
            double outerScalarDistance = shapeEdgeSelector.ResolveDistance(outerDistance);

            DT distance = new DT();
            shapeEdgeSelector.InitDistance(ref distance);

            int winding = 0;
            if (innerScalarDistance >= 0 && Math.Abs(innerScalarDistance) <= Math.Abs(outerScalarDistance))
            {
                distance = innerDistance;
                winding = 1;
                for (int i = 0; i < contourCount; ++i)
                {
                    if (windings[i] > 0)
                    {
                        DT contourDistance = edgeSelectors[i].Distance();
                        if (Math.Abs(edgeSelectors[i].ResolveDistance(contourDistance)) < Math.Abs(outerScalarDistance) && edgeSelectors[i].ResolveDistance(contourDistance) > edgeSelectors[i].ResolveDistance(distance))
                            distance = contourDistance;
                    }
                }
            }
            else if (outerScalarDistance <= 0 && Math.Abs(outerScalarDistance) < Math.Abs(innerScalarDistance))
            {
                distance = outerDistance;
                winding = -1;
                for (int i = 0; i < contourCount; ++i)
                {
                    if (windings[i] < 0)
                    {
                        DT contourDistance = edgeSelectors[i].Distance();
                        if (Math.Abs(edgeSelectors[i].ResolveDistance(contourDistance)) < Math.Abs(innerScalarDistance) && edgeSelectors[i].ResolveDistance(contourDistance) < edgeSelectors[i].ResolveDistance(distance))
                            distance = contourDistance;
                    }
                }
            }
            else
            {
                return shapeDistance;
            }

            for (int i = 0; i < contourCount; ++i)
            {
                if (windings[i] != winding)
                {
                    DT contourDistance = edgeSelectors[i].Distance();
                    if (edgeSelectors[i].ResolveDistance(contourDistance) * edgeSelectors[i].ResolveDistance(distance) >= 0 && Math.Abs(edgeSelectors[i].ResolveDistance(contourDistance)) < Math.Abs(edgeSelectors[i].ResolveDistance(distance)))
                        distance = contourDistance;
                }
            }

            if (shapeEdgeSelector.ResolveDistance(distance) == shapeEdgeSelector.ResolveDistance(shapeDistance))
                distance = shapeDistance;

            return distance;
        }
    }
}
