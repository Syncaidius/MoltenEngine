using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.MSDF
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="ES">Edge selector type</typeparam>
    /// <typeparam name="DT">Distance Type</typeparam>
    public class ShapeDistanceFinder<ES, DT, EC>
        where ES : EdgeSelector<DT, EC>, new()
        where DT : unmanaged
        where EC : unmanaged
    {
        MsdfShape shape;
        ContourCombiner<ES, DT, EC> contourCombiner;
        EC[] shapeEdgeCache;

        public ShapeDistanceFinder(MsdfShape shape, ContourCombiner<ES, DT, EC> combiner)
        {
            this.shape = shape;
            contourCombiner = combiner;
            shapeEdgeCache = new EC[shape.edgeCount()];
        }

        public DT distance(in Vector2D origin)
        {
            contourCombiner.reset(origin);

            int ecIndex = 0;
            ref EC edgeCache = ref shapeEdgeCache[ecIndex];

            for (int i = 0; i < shape.Contours.Count; i++)
            {
                Contour contour = shape.Contours[i];
                int edgeCount = contour.Edges.Count;

                if (edgeCount > 0)
                {
                    ES edgeSelector = contourCombiner.edgeSelector(i);

                    EdgeSegment prevEdge = contour.Edges.Count >= 2 ? (contour.Edges[edgeCount - 2]).Segment : contour.Edges[0].Segment;
                    EdgeSegment curEdge = contour.Edges.Last().Segment;
                    foreach (EdgeHolder edge in contour.Edges)
                    {
                        EdgeSegment nextEdge = edge.Segment;
                        edgeSelector.addEdge(ref edgeCache, prevEdge, curEdge, nextEdge);
                        ecIndex++;
                        prevEdge = curEdge;
                        curEdge = nextEdge;
                    }
                }
            }

            return contourCombiner.distance();
        }

        public DT oneShotDistance(ContourCombiner<ES, DT, EC> combiner, MsdfShape shape, in Vector2D origin) {
            contourCombiner = combiner;
            contourCombiner.reset(origin);

            for (int i = 0; i < shape.Contours.Count; i++)
            {
                Contour contour = shape.Contours[i];
                int edgeCount = contour.Edges.Count;

                if (edgeCount > 0)
                {
                    ES edgeSelector = contourCombiner.edgeSelector(i);

                    EdgeSegment prevEdge = contour.Edges.Count >= 2 ? (contour.Edges[edgeCount - 2]).Segment : contour.Edges[0].Segment;
                    EdgeSegment curEdge = contour.Edges.Last().Segment;

                    foreach (EdgeHolder edge in contour.Edges)
                    { 
                        EdgeSegment nextEdge = edge.Segment;
                        EC dummy = new EC();
                        edgeSelector.addEdge(ref dummy, prevEdge, curEdge, nextEdge);
                        prevEdge = curEdge;
                        curEdge = nextEdge;
                    }
                }
            }

            return contourCombiner.distance();
        }
    }

    public class SimpleTrueShapeDistanceFinder : ShapeDistanceFinder<TrueDistanceSelector, double, TrueDistanceSelector.EdgeCache>
    {
        public SimpleTrueShapeDistanceFinder(MsdfShape shape) :
            base(shape, new SimpleContourCombiner<TrueDistanceSelector, double, TrueDistanceSelector.EdgeCache>(shape))
        {
        }
    }
}
