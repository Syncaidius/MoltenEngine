using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.MSDF
{
    public class MsdfShape
    {
        const double MSDFGEN_CORNER_DOT_EPSILON = 0.000001;
        const double MSDFGEN_DECONVERGENCE_FACTOR = 0.000001;

        public struct Bounds
        {
            public double l, b, r, t;

            public Bounds(double ll, double bb, double rr, double tt)
            {
                l = ll;
                b = bb;
                r = rr; 
                t = tt;
            }
        }

        struct Intersection
        {
            public double x;
            public int direction;
            public int contourIndex;

            public Intersection(double pX, int pDirection, int pContourIndex)
            {
                x = pX;
                direction = pDirection;
                contourIndex = pContourIndex;
            }

            public static int compare(Intersection a, Intersection b)
            {
                return MsdfMath.sign(a.x - b.x);
            }
        };

        /// <summary>
        /// The list of contours the shape consists of.
        /// </summary>
        public List<Contour> Contours = new List<Contour>();

        /// <summary>
        /// Specifies whether the shape uses bottom-to-top (false) or top-to-bottom (true) Y coordinates.
        /// </summary>
        public bool InverseYAxis = false;

        /// <summary>
        /// Adds a contour.
        /// </summary>
        /// <param name="contour"></param>
        void AddContour(Contour contour)
        {
            Contours.Add(contour);
        }

        /// <summary>
        /// Adds a blank contour and returns its reference.
        /// </summary>
        /// <returns></returns>
        public Contour AddContour()
        {
            Contour c = new Contour();
            Contours.Add(c);
            return c;
        }

        /// <summary>
        /// Normalizes the shape geometry for distance field generation.
        /// </summary>
        public void Normalize()
        {
            foreach (Contour contour in Contours)
            {
                if (contour.Edges.Count == 1)
                {
                    EdgeSegment[] parts = new EdgeSegment[3];  
                    contour.Edges[0].splitInThirds(ref parts[0], ref parts[1], ref parts[2]);
                    contour.Edges.Clear();
                    contour.Edges.Add(parts[0]);
                    contour.Edges.Add(parts[1]);
                    contour.Edges.Add(parts[2]);
                }
                else
                {
                    EdgeSegment prevEdge = contour.Edges.Last();
                    foreach(EdgeSegment edge in contour.Edges)
                    {
                        Vector2D prevDir = edge.direction(1).GetNormalized();
                        Vector2D curDir = edge.direction(0).GetNormalized();
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

        private static void DeconvergeEdge(EdgeSegment edge, int param)
        {
            {
                if(edge is QuadraticSegment quadraticSegment)
                    edge = quadraticSegment.ConvertToCubic();
            }
            {
                if(edge is CubicSegment cubicSegment)
                    cubicSegment.Deconverge(param, MSDFGEN_DECONVERGENCE_FACTOR);
            }
        }

        /// <summary>
        /// Performs basic checks to determine if the object represents a valid shape.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            foreach (Contour contour in Contours)
            {
                if (contour.Edges.Count > 0)
                {
                    Vector2D corner = contour.Edges.Last().point(1);
                    foreach (EdgeSegment edge in contour.Edges)
                    {
                        if (edge == null)
                            return false;
                        if (edge.point(0) != corner)
                            return false;

                        corner = edge.point(1);
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Adjusts the bounding box to fit the shape.
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public void bound(double l, double b, double r, double t)
        {
            foreach (Contour contour in Contours)
                contour.bound(ref l, ref b, ref r, ref t);
        }

        /// <summary>
        /// Adjusts the bounding box to fit the shape border's mitered corners.
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name="border"></param>
        /// <param name="miterLimit"></param>
        /// <param name="polarity"></param>
        public void boundMiters(double l, double b, double r, double t, double border, double miterLimit, int polarity)
        {
            foreach (Contour contour in Contours)
                contour.boundMiters(ref l, ref b, ref r, ref t, border, miterLimit, polarity);
        }

        /// <summary>
        /// Computes the minimum bounding box that fits the shape, optionally with a (mitered) border.
        /// </summary>
        /// <param name="border"></param>
        /// <param name="miterLimit"></param>
        /// <param name="polarity"></param>
        /// <returns></returns>
        public Bounds getBounds(double border = 0, double miterLimit = 0, int polarity = 0)
        {
            const double LARGE_VALUE = 1e240;
            MsdfShape.Bounds bounds = new Bounds( +LARGE_VALUE, +LARGE_VALUE, -LARGE_VALUE, -LARGE_VALUE);
            bound(bounds.l, bounds.b, bounds.r, bounds.t);
            if (border > 0)
            {
                bounds.l -= border;
                bounds.b -= border;
                bounds.r += border;
                bounds.t += border;

                if (miterLimit > 0)
                    boundMiters(bounds.l, bounds.b, bounds.r, bounds.t, border, miterLimit, polarity);
            }
            return bounds;
        }

        /// <summary>
        /// Outputs the scanline that intersects the shape at y.
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name="y"></param>
        public unsafe void scanline(Scanline line, double y) {
            List<Scanline.Intersection> intersections = new List<Scanline.Intersection>();
            double* x = stackalloc double[3];
            int* dy = stackalloc int[3];
            foreach (Contour contour in Contours)
            {
                foreach (EdgeSegment edge in contour.Edges)
                {
                    int n = edge.scanlineIntersections(x, dy, y);
                    for (int i = 0; i < n; ++i)
                    {
                        Scanline.Intersection intersection = new Scanline.Intersection(x[i], dy[i]);
                        intersections.Add(intersection);
                    }
                }
            }

            line.setIntersections(intersections);
        }

        /// <summary>
        /// Returns the total number of edge segments
        /// </summary>
        /// <returns></returns>
        public int edgeCount()
        {
            int total = 0;
            foreach (Contour contour in Contours)
                total += (int)contour.Edges.Count;
            return total;
        }

        /// <summary>
        /// Assumes its contours are unoriented (even-odd fill rule). Attempts to orient them to conform to the non-zero winding rule.
        /// </summary>
        public unsafe void orientContours()
        {
            double ratio = 0.5 * (Math.Sqrt(5) - 1); // an irrational number to minimize chance of intersecting a corner or other point of interest
            int[] orientations = new int[Contours.Count];
            List<Intersection> intersections = new List<Intersection>();

            for (int i = 0; i < Contours.Count; ++i) {
                if (orientations[i] == 0 && Contours[i].Edges.Count > 0) {
                    // Find an Y that crosses the contour
                    double y0 = Contours[i].Edges.First().point(0).Y;
                    double y1 = y0;

                    for (int j = 0; j < Contours[i].Edges.Count && y0 == y1; j++) 
                        y1 = Contours[i].Edges[j].point(1).Y;
                    for (int j = 0; j < Contours[i].Edges.Count && y0 == y1; j++)
                        y1 = Contours[i].Edges[j].point(ratio).Y; // in case all endpoints are in a horizontal line
                    double y = MsdfMath.mix(y0, y1, ratio);
                    // Scanline through whole shape at Y
                    double* x = stackalloc double[3];
                    int* dy = stackalloc int[3];
                    for (int j = 0; j < Contours.Count; ++j) 
                    {
                        foreach (EdgeSegment edge in Contours[j].Edges)
                        { 
                            int n = edge.scanlineIntersections(x, dy, y);
                            for (int k = 0; k < n; ++k) {
                                Intersection intersection = new Intersection( x[k], dy[k], j );
                                intersections.Add(intersection);
                            }
                        }
                    }
                    intersections.Sort(Intersection.compare);
                    // Disqualify multiple intersections
                    for (int j = 1; j < intersections.Count; ++j)
                        if (intersections[j].x == intersections[j - 1].x)
                        {
                            Intersection isec = intersections[j];
                            Intersection isecPrev = intersections[j - 1];

                            isec.direction = 0;
                            isecPrev.direction = 0;

                            intersections[j] = isec;
                            intersections[j - 1] = isecPrev;
                        }
                    // Inspect scanline and deduce orientations of intersected contours
                    for (int j = 0; j < intersections.Count; ++j)
                        if (intersections[j].direction != 0)
                            orientations[intersections[j].contourIndex] += 2 * ((j & 1) ^ (intersections[j].direction > 0 ? 1 : 0)) - 1;
                    intersections.Clear();
                }
            }
            // Reverse contours that have the opposite orientation
            for (int i = 0; i < Contours.Count; ++i) {
                if (orientations[i] < 0)
                    Contours[i].reverse();
            }
        }
    }
}
