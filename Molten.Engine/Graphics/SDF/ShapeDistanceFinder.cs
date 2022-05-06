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
    /// <typeparam name="ES">Edge selector type</typeparam>
    /// <typeparam name="DT">Distance Type</typeparam>
    public class ShapeDistanceFinder
    {
        Shape shape;
        ContourCombiner contourCombiner;
        EdgeCache[] shapeEdgeCache;

        public ShapeDistanceFinder(Shape shape)
        {
            this.shape = shape;
            contourCombiner = new ContourCombiner(shape);
            shapeEdgeCache = new EdgeCache[shape.GetEdgeCount()];
        }

        public MultiDistance distance(ref Vector2D origin)
        {
            contourCombiner.Reset(ref origin);

            int edgeCache = 0;

            for (int i = 0; i < shape.Contours.Count; i++)
            {
                Shape.Contour contour = shape.Contours[i];
                int edgeCount = contour.Edges.Count;

                if (edgeCount > 0)
                {
                    MultiDistanceSelector edgeSelector = contourCombiner.EdgeSelectors[0];

                    Shape.Edge prevEdge = contour.Edges.Count >= 2 ? (contour.Edges[edgeCount - 2]) : contour.Edges[0];
                    Shape.Edge curEdge = contour.Edges.Last();
                    foreach (Shape.Edge edge in contour.Edges)
                    {
                        Shape.Edge nextEdge = edge;
                        edgeSelector.AddEdge(ref shapeEdgeCache[edgeCache++], prevEdge, curEdge, nextEdge);
                        prevEdge = curEdge;
                        curEdge = nextEdge;
                    }
                }
            }

            return contourCombiner.Distance();
        }

        public float getRefPSD(ref Vector2D origin, double invRange)
        {
            MultiDistance dist = distance(ref origin);

            MultiDistanceSelector es = new MultiDistanceSelector();

            return es.GetRefPSD(ref dist, invRange);
        }
    }
}
