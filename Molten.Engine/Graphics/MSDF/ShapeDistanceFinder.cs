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
        MsdfShape shape;
        ContourCombiner<ES, DT> contourCombiner;
        EdgeCache[] shapeEdgeCache;

        public ShapeDistanceFinder(MsdfShape shape, ContourCombiner<ES, DT> combiner)
        {
            this.shape = shape;
            contourCombiner = combiner;
            shapeEdgeCache = new EdgeCache[shape.edgeCount()];
        }

        public DT distance(ref Vector2D origin)
        {
            contourCombiner.Reset(ref origin);

            int ecIndex = 0;
            ref EdgeCache edgeCache = ref shapeEdgeCache[ecIndex];

            for (int i = 0; i < shape.Contours.Count; i++)
            {
                Contour contour = shape.Contours[i];
                int edgeCount = contour.Edges.Count;

                if (edgeCount > 0)
                {
                    ES edgeSelector = contourCombiner.EdgeSelector(i);

                    EdgeSegment prevEdge = contour.Edges.Count >= 2 ? (contour.Edges[edgeCount - 2]) : contour.Edges[0];
                    EdgeSegment curEdge = contour.Edges.Last();
                    foreach (EdgeSegment edge in contour.Edges)
                    {
                        EdgeSegment nextEdge = edge;
                        edgeSelector.AddEdge(ref edgeCache, prevEdge, curEdge, nextEdge);
                        ecIndex++;
                        prevEdge = curEdge;
                        curEdge = nextEdge;
                    }
                }
            }

            return contourCombiner.Distance();
        }

        public DT oneShotDistance(ContourCombiner<ES, DT> combiner, MsdfShape shape, ref Vector2D origin) {
            contourCombiner = combiner;
            contourCombiner.Reset(ref origin);

            for (int i = 0; i < shape.Contours.Count; i++)
            {
                Contour contour = shape.Contours[i];
                int edgeCount = contour.Edges.Count;

                if (edgeCount > 0)
                {
                    ES edgeSelector = contourCombiner.EdgeSelector(i);

                    EdgeSegment prevEdge = contour.Edges.Count >= 2 ? (contour.Edges[edgeCount - 2]) : contour.Edges[0];
                    EdgeSegment curEdge = contour.Edges.Last();

                    foreach (EdgeSegment edge in contour.Edges)
                    { 
                        EdgeSegment nextEdge = edge;
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

    public class SimpleTrueShapeDistanceFinder : ShapeDistanceFinder<TrueDistanceSelector, double>
    {
        public SimpleTrueShapeDistanceFinder(MsdfShape shape) :
            base(shape, new SimpleContourCombiner<TrueDistanceSelector, double>(shape))
        {
        }
    }
}
