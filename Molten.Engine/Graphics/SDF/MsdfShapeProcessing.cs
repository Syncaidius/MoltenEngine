using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public static class MsdfShapeProcessing
    {
        const double MSDFGEN_CORNER_DOT_EPSILON = 0.000001;
        const double MSDFGEN_DECONVERGENCE_FACTOR = 0.000001;

        /// <summary>
        /// Outputs the scanline that intersects the shape at y.
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name="y"></param>
        public static unsafe void scanline(Shape shape, Scanline line, double y)
        {
            List<Scanline.Intersection> intersections = new List<Scanline.Intersection>();
            double* x = stackalloc double[3];
            int* dy = stackalloc int[3];
            foreach (Shape.Contour contour in shape.Contours)
            {
                foreach (Shape.Edge edge in contour.Edges)
                {
                    int n = edge.ScanlineIntersections(x, dy, y);
                    for (int i = 0; i < n; ++i)
                    {
                        Scanline.Intersection intersection = new Scanline.Intersection(x[i], dy[i]);
                        intersections.Add(intersection);
                    }
                }
            }

            line.SetIntersections(intersections);
        }

        /// <summary>
        /// Normalizes the shape geometry for distance field generation.
        /// </summary>
        public static void Normalize(Shape shape)
        {
            foreach (Shape.Contour contour in shape.Contours)
            {
                if (contour.Edges.Count == 1)
                {
                    Shape.Edge[] parts = new Shape.Edge[3];
                    contour.Edges[0].SplitInThirds(ref parts[0], ref parts[1], ref parts[2]);
                    contour.Edges.Clear();
                    contour.Edges.Add(parts[0]);
                    contour.Edges.Add(parts[1]);
                    contour.Edges.Add(parts[2]);
                }
                else
                {
                    Shape.Edge prevEdge = contour.Edges.Last();
                    foreach (Shape.Edge edge in contour.Edges)
                    {
                        Vector2D prevDir = prevEdge.GetDirection(1).GetNormalized();
                        Vector2D curDir = edge.GetDirection(0).GetNormalized();
                        if (Vector2D.Dot(prevDir, curDir) < MSDFGEN_CORNER_DOT_EPSILON - 1)
                        {
                            DeconvergeEdge(prevEdge, 1);
                            DeconvergeEdge(edge, 0);
                        }
                        prevEdge = edge;
                    }
                }
            }
        }

        private static void DeconvergeCubicEdge(Shape.CubicEdge edge, int param, double amount)
        {
            Vector2D dir = edge.GetDirection(param);
            Vector2D normal = dir.GetOrthonormal();
            double h = Vector2D.Dot(edge.GetDirectionChange(param) - dir, normal);
            switch (param)
            {
                case 0:
                    edge.p[Shape.Edge.P1] += amount * (dir + Math.Sign(h) * Math.Sqrt(Math.Abs(h)) * normal);
                    break;
                case 1:
                    edge.p[Shape.Edge.CP1] -= amount * (dir - Math.Sign(h) * Math.Sqrt(Math.Abs(h)) * normal);
                    break;
            }
        }

        private static void DeconvergeEdge(Shape.Edge edge, int param)
        {
            {
                if (edge is Shape.QuadraticEdge quadratic)
                    edge = quadratic.ConvertToCubic();
            }
            {
                if (edge is Shape.CubicEdge cubic)
                    DeconvergeCubicEdge(cubic, param, MSDFGEN_DECONVERGENCE_FACTOR);
            }
        }
    }
}
