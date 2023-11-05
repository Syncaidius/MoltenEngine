using Molten.DoublePrecision;
using Molten.Shapes;

namespace Molten.Graphics.SDF
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ES">Edge selector type</typeparam>
    /// <typeparam name="DT">Distance Type</typeparam>
    public class ShapeDistanceFinder
    {
        Shape _shape;
        ContourCombiner _combiner;
        EdgeCache[] _EdgeCache;

        public ShapeDistanceFinder(Shape shape)
        {
            _shape = shape;
            _combiner = new ContourCombiner(shape);
            _EdgeCache = new EdgeCache[shape.GetEdgeCount()];
        }

        public Color3D distance(ref Vector2D origin)
        {
            _combiner.Reset(ref origin);

            int edgeCache = 0;

            for (int i = 0; i < _shape.Contours.Count; i++)
            {
                Contour contour = _shape.Contours[i];
                int edgeCount = contour.Edges.Count;

                if (edgeCount > 0)
                {
                    MultiDistanceSelector edgeSelector = _combiner.EdgeSelectors[0];

                    Edge prevEdge = contour.Edges.Count >= 2 ? (contour.Edges[edgeCount - 2]) : contour.Edges[0];
                    Edge curEdge = contour.Edges.Last();
                    foreach (Edge edge in contour.Edges)
                    {
                        Edge nextEdge = edge;
                        edgeSelector.AddEdge(ref _EdgeCache[edgeCache++], prevEdge, curEdge, nextEdge);
                        prevEdge = curEdge;
                        curEdge = nextEdge;
                    }
                }
            }

            return _combiner.Distance();
        }

        public float getRefPSD(ref Vector2D origin, double invRange)
        {
            Color3D dist = distance(ref origin);
            MultiDistanceSelector es = new MultiDistanceSelector();

            return es.GetRefPSD(ref dist, invRange);
        }
    }
}
