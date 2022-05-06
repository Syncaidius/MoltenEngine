using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.SDF
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ES">EdgeSelector type.</typeparam>
    /// <typeparam name="DT">Distance type. e.g. double, MultiDistance or MultiAndTrueDistance.</typeparam>
    /// <typeparam name="EC">Edge cache type.</typeparam>
    public class ContourCombiner<ES, DT>
         where ES : EdgeSelector<DT>, new()
        where DT : unmanaged
    {
        Vector2D p;
        List<int> windings;
        internal List<ES> EdgeSelectors { get; }

        public ContourCombiner(Shape shape)
        {
            windings = new List<int>(shape.Contours.Count);
            foreach (Shape.Contour contour in shape.Contours)
                windings.Add(contour.GetWinding());

            EdgeSelectors = new List<ES>(shape.Contours.Count);
            for (int i = 0; i < shape.Contours.Count; i++)
                EdgeSelectors.Add(new ES());
        }

        public void Reset(ref Vector2D p)
        {
            this.p = p;
            foreach (EdgeSelector<DT> contourEdgeSelector in EdgeSelectors)
                contourEdgeSelector.Reset(ref p);
        }

        public DT Distance()
        {
            int contourCount = EdgeSelectors.Count;
            ES shapeEdgeSelector = new ES();
            ES innerEdgeSelector = new ES();
            ES outerEdgeSelector = new ES();

            shapeEdgeSelector.Reset(ref p);
            innerEdgeSelector.Reset(ref p);
            outerEdgeSelector.Reset(ref p);

            for (int i = 0; i < contourCount; ++i)
            {
                DT edgeDistance = EdgeSelectors[i].Distance();
                shapeEdgeSelector.Merge(EdgeSelectors[i]);
                if (windings[i] > 0 && EdgeSelectors[i].ResolveDistance(edgeDistance) >= 0)
                    innerEdgeSelector.Merge(EdgeSelectors[i]);
                if (windings[i] < 0 && EdgeSelectors[i].ResolveDistance(edgeDistance) <= 0)
                    outerEdgeSelector.Merge(EdgeSelectors[i]);
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
                        DT contourDistance = EdgeSelectors[i].Distance();
                        if (Math.Abs(EdgeSelectors[i].ResolveDistance(contourDistance)) < Math.Abs(outerScalarDistance) &&
                            EdgeSelectors[i].ResolveDistance(contourDistance) > EdgeSelectors[i].ResolveDistance(distance))
                        {
                            distance = contourDistance;
                        }
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
                        DT contourDistance = EdgeSelectors[i].Distance();
                        if (Math.Abs(EdgeSelectors[i].ResolveDistance(contourDistance)) < Math.Abs(innerScalarDistance) &&
                            EdgeSelectors[i].ResolveDistance(contourDistance) < EdgeSelectors[i].ResolveDistance(distance))
                        {
                            distance = contourDistance;
                        }
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
                    DT contourDistance = EdgeSelectors[i].Distance();
                    if (EdgeSelectors[i].ResolveDistance(contourDistance) * EdgeSelectors[i].ResolveDistance(distance) >= 0 &&
                        Math.Abs(EdgeSelectors[i].ResolveDistance(contourDistance)) < Math.Abs(EdgeSelectors[i].ResolveDistance(distance)))
                    {
                        distance = contourDistance;
                    }
                }
            }

            if (shapeEdgeSelector.ResolveDistance(distance) == shapeEdgeSelector.ResolveDistance(shapeDistance))
                distance = shapeDistance;

            return distance;
        }
    }
}
