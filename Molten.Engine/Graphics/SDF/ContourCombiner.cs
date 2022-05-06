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
    public class ContourCombiner
    {
        Vector2D p;
        List<int> windings;
        internal List<MultiDistanceSelector> EdgeSelectors { get; }

        public ContourCombiner(Shape shape)
        {
            windings = new List<int>(shape.Contours.Count);
            foreach (Shape.Contour contour in shape.Contours)
                windings.Add(contour.GetWinding());

            EdgeSelectors = new List<MultiDistanceSelector>(shape.Contours.Count);
            for (int i = 0; i < shape.Contours.Count; i++)
                EdgeSelectors.Add(new MultiDistanceSelector());
        }

        public void Reset(ref Vector2D p)
        {
            this.p = p;
            foreach (MultiDistanceSelector contourEdgeSelector in EdgeSelectors)
                contourEdgeSelector.Reset(ref p);
        }

        public MultiDistance Distance()
        {
            int contourCount = EdgeSelectors.Count;
            MultiDistanceSelector shapeEdgeSelector = new MultiDistanceSelector();
            MultiDistanceSelector innerEdgeSelector = new MultiDistanceSelector();
            MultiDistanceSelector outerEdgeSelector = new MultiDistanceSelector();

            shapeEdgeSelector.Reset(ref p);
            innerEdgeSelector.Reset(ref p);
            outerEdgeSelector.Reset(ref p);

            for (int i = 0; i < contourCount; ++i)
            {
                MultiDistance edgeDistance = EdgeSelectors[i].Distance();
                shapeEdgeSelector.Merge(EdgeSelectors[i]);
                if (windings[i] > 0 && EdgeSelectors[i].ResolveDistance(edgeDistance) >= 0)
                    innerEdgeSelector.Merge(EdgeSelectors[i]);
                if (windings[i] < 0 && EdgeSelectors[i].ResolveDistance(edgeDistance) <= 0)
                    outerEdgeSelector.Merge(EdgeSelectors[i]);
            }

            MultiDistance shapeDistance = shapeEdgeSelector.Distance();
            MultiDistance innerDistance = innerEdgeSelector.Distance();
            MultiDistance outerDistance = outerEdgeSelector.Distance();

            double innerScalarDistance = shapeEdgeSelector.ResolveDistance(innerDistance);
            double outerScalarDistance = shapeEdgeSelector.ResolveDistance(outerDistance);

            MultiDistance distance = new MultiDistance();
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
                        MultiDistance contourDistance = EdgeSelectors[i].Distance();
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
                        MultiDistance contourDistance = EdgeSelectors[i].Distance();
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
                    MultiDistance contourDistance = EdgeSelectors[i].Distance();
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
