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
    /// <typeparam name="ES">Edge selector type</typeparam>
    /// <typeparam name="DT">Distance Type</typeparam>
    public class ShapeDistanceFinder<ES, DT>
        where ES : EdgeSelector<DT>, new()
        where DT : unmanaged
    {
        ContourShape shape;
        ContourCombiner<ES, DT> contourCombiner;
        EdgeCache[] shapeEdgeCache;

        public ShapeDistanceFinder(ContourShape shape, ContourCombiner<ES, DT> combiner)
        {
            this.shape = shape;
            contourCombiner = combiner;
            shapeEdgeCache = new EdgeCache[shape.GetEdgeCount()];
        }

        public DT distance(ref Vector2D origin)
        {
            contourCombiner.Reset(ref origin);

            int ecIndex = 0;
            ref EdgeCache edgeCache = ref shapeEdgeCache[ecIndex];

            for (int i = 0; i < shape.Contours.Count; i++)
            {
                ContourShape.Contour contour = shape.Contours[i];
                int edgeCount = contour.Edges.Count;

                if (edgeCount > 0)
                {
                    ES edgeSelector = contourCombiner.EdgeSelector(i);

                    ContourShape.Edge prevEdge = contour.Edges.Count >= 2 ? (contour.Edges[edgeCount - 2]) : contour.Edges[0];
                    ContourShape.Edge curEdge = contour.Edges.Last();
                    foreach (ContourShape.Edge edge in contour.Edges)
                    {
                        ContourShape.Edge nextEdge = edge;
                        edgeSelector.AddEdge(ref edgeCache, prevEdge, curEdge, nextEdge);
                        ecIndex++;
                        prevEdge = curEdge;
                        curEdge = nextEdge;
                    }
                }
            }

            return contourCombiner.Distance();
        }

        public DT oneShotDistance(ContourCombiner<ES, DT> combiner, ContourShape shape, ref Vector2D origin) {
            contourCombiner = combiner;
            contourCombiner.Reset(ref origin);

            for (int i = 0; i < shape.Contours.Count; i++)
            {
                ContourShape.Contour contour = shape.Contours[i];
                int edgeCount = contour.Edges.Count;

                if (edgeCount > 0)
                {
                    ES edgeSelector = contourCombiner.EdgeSelector(i);

                    ContourShape.Edge prevEdge = contour.Edges.Count >= 2 ? (contour.Edges[edgeCount - 2]) : contour.Edges[0];
                    ContourShape.Edge curEdge = contour.Edges.Last();

                    foreach (ContourShape.Edge edge in contour.Edges)
                    {
                        ContourShape.Edge nextEdge = edge;
                        EdgeCache dummy = new EdgeCache();
                        edgeSelector.AddEdge(ref dummy, prevEdge, curEdge, nextEdge);
                        prevEdge = curEdge;
                        curEdge = nextEdge;
                    }
                }
            }

            return contourCombiner.Distance();
        }

        public float getRefPSD(ref Vector2D origin, double invRange)
        {
            DT dist = distance(ref origin);
            
            ES es = new ES();

            return es.GetRefPSD(ref dist, invRange);
        }
    }
}
