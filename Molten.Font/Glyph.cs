using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class Glyph : ICloneable
    {
        Rectangle _bounds;
        GlyphPoint[] _points;

        public static readonly Glyph Empty = new Glyph(Rectangle.Empty, new ushort[0], new GlyphPoint[0], new byte[0]);

        public Rectangle Bounds
        {
            get => _bounds;
            internal set => _bounds = value;
        }

        public int MinX => _bounds.X;

        public int MinY => _bounds.Y;

        public int MaxX => _bounds.Right;

        public int MaxY => _bounds.Bottom;

        public ushort[] ContourEndPoints { get; private set; }

        public GlyphPoint[] Points => _points;

        public byte[] Instructions { get; private set; }

        internal Glyph(Rectangle bounds, ushort[] contourEndPoints, GlyphPoint[] points, byte[] instructions)
        {
            Bounds = bounds;
            ContourEndPoints = contourEndPoints;
            Instructions = instructions;
            _points = points;
        }

        /// <summary>
        /// Append the data from the provided glyph to the current glyph. This is useful when building a composite glyph.
        /// </summary>
        /// <param name="other">The glyph to be appended to the current one</param>
        internal void Append(Glyph other)
        {
            int oldLength = ContourEndPoints.Length;
            int src_contour_count = other.ContourEndPoints.Length;
            ushort oldLastPointCount = (ushort)(ContourEndPoints[oldLength - 1] + 1);

            _points = ArrayHelper.Concat(Points, other.Points);
            ContourEndPoints = ArrayHelper.Concat(ContourEndPoints, other.ContourEndPoints);
            int newLength = ContourEndPoints.Length;

            // Offset appended end-points by the current glyph's original end point count.
            for (int i = oldLength; i < newLength; ++i)
                ContourEndPoints[i] += oldLastPointCount;

            Bounds = Rectangle.Union(Bounds, other.Bounds);
        }

        public Glyph Clone()
        {
            ushort[] contourClone = new ushort[ContourEndPoints.Length];
            Array.Copy(ContourEndPoints, contourClone, contourClone.Length);

            GlyphPoint[] pointClone = new GlyphPoint[Points.Length];
            Array.Copy(Points, pointClone, Points.Length);

            byte[] instructionClone = new byte[Instructions.Length];
            Array.Copy(Instructions, instructionClone, instructionClone.Length);
            return new Glyph(Bounds, contourClone, pointClone, instructionClone);
        }

        /// <summary>
        /// Populates the <see cref="Shapes"/> list based on the gylph's outline.
        /// </summary>
        /// <param name="pointsPerCurve">The maximum number of points per curve in a glyph contour.</param>
        /// <returns></returns>
        public List<Shape> CreateShapes(int pointsPerCurve, bool flipYAxis)
        {
            List<Shape> result = new List<Shape>();
            List<Shape> holes = new List<Shape>();
            List<Vector2F> cp = new List<Vector2F>();
            Vector2F prevCurvePoint = Vector2F.Zero;
            int start = 0;
            GlyphPoint p = GlyphPoint.Empty;

            for (int i = 0; i < ContourEndPoints.Length; i++)
            {
                Shape shape = new Shape();
                int end = ContourEndPoints[i];
                float curveIncrement = 1.0f / pointsPerCurve;
                cp.Clear();

                RectangleF pointBounds = new RectangleF();
                for (int j = start; j <= end; j++)
                {
                    p = _points[j];
                    pointBounds.Encapsulate(p.Point);

                    // If off curve, it's a bezier control point.
                    if (p.IsOnCurve)
                    {
                        PlotCurve(shape, prevCurvePoint, p.Point, cp, pointsPerCurve, curveIncrement);
                        prevCurvePoint = p.Point;

                    }
                    else
                    {
                        cp.Add(p.Point);
                    }
                }

                int winding = GetWinding(start, end, new Vector2F(_bounds.Center.X, _bounds.Center.Y));

                // Close contour, by linking the end point back to the start point.
                if (cp.Count > 0)
                    PlotCurve(shape, prevCurvePoint, (Vector2F)shape.Points[0], cp, pointsPerCurve, curveIncrement);
                else
                    shape.Points.Add(new TriPoint((Vector2F)shape.Points[0]));

                // Add the first point again to create a loop (for rendering only)
                shape.CalculateBounds();

                // Flip points
                if (flipYAxis)
                {
                    for (int j = 0; j < shape.Points.Count; j++)
                    {
                        TriPoint tp = shape.Points[j];
                        tp.Y = _bounds.Height - shape.Points[j].Y;
                        shape.Points[j] = tp;
                    }
                }

                holes.Add(shape);

                start = end + 1;
            }

            holes.Sort(Shape.AreaComparer);
            //holes.Reverse(); // Flip to descending area size.

            // TODO replace with polygon intersect - letter C in UECHIGOT.TTF

            // Figure out which holes belong to which shape
            foreach (Shape h in holes)
            {
                bool holeContained = false;
                foreach (Shape s in result)
                {
                    if (s.Bounds.Contains(h.Bounds))
                    {
                        s.Holes.Add(h);
                        holeContained = true;
                        break;
                    }
                }

                // Must be an outline, reverse it's winding.
                if (!holeContained)
                {
                    h.Points.Reverse();
                    result.Add(h);
                }
            }

            return result;
        }

        private int GetWinding(int start, int end, Vector2F center)
        {
            Vector2F p1;
            Vector2F p2;
            int windingNumber = 0;

            Ray originRay = new Ray(new Vector3F(center, 0), Vector3F.Right);
            Ray contourRay;
            Vector3F intersect;

            for(int i = start; i < end; i++)
            {
                p1 = _points[i].Point;
                p2 = _points[i + 1].Point;

                RectangleF contourBounds = new RectangleF();
                contourBounds.Encapsulate(p1);
                contourBounds.Encapsulate(p2);

                Vector2F dir = p2 - p1;
                contourRay = new Ray(new Vector3F(p1,0), new Vector3F(dir, 0));
                if (CollisionHelper.RayIntersectsRay(ref originRay, ref contourRay, out intersect))
                {
                    if (contourBounds.Contains(new Vector2F(intersect.X, intersect.Y)))
                    {
                        // Glyph points are defined with origin 0,0 at the bottom left. Ours is top left, so handle accordingly.
                        // Upward intersection
                        if (p2.Y > p1.Y)
                        {
                            windingNumber--;
                            // TODO check if between X of p1 and p2. If not, ignore the intersection.
                        }
                        else if(p2.Y < p1.Y) // downward intersection
                        {
                            windingNumber++;
                        }
                        else // Test right-to-left or left-to-right
                        {
                            if (p2.X < p1.X) // right-to-left crossing
                                windingNumber--;
                            else if (p2.X > p1.X)
                                windingNumber++;
                        }
                    }
                }
            }

            return windingNumber;
        }

        private void PlotCurve(Shape shape, Vector2F prevPoint, Vector2F curPoint, List<Vector2F> cp, float pointsPerCurve, float curveIncrement)
        {
            float curvePercent = 0f;
            switch (cp.Count)
            {
                case 0: // Line
                    shape.Points.Add(new TriPoint(curPoint));
                    break;

                case 1: // Quadratic bezier curve
                    curvePercent = 0f;
                    for (int c = 0; c < pointsPerCurve; c++)
                    {
                        curvePercent += curveIncrement;
                        Vector2F cPos = BezierCurve2D.CalculateQuadratic(curvePercent, prevPoint, curPoint, cp[0]);
                        shape.Points.Add(new TriPoint(cPos));
                    }
                    break;

                case 2: // Cubic curve
                    curvePercent = 0f;
                    for (int c = 0; c < pointsPerCurve; c++)
                    {
                        curvePercent += curveIncrement;
                        Vector2F cPos = BezierCurve2D.CalculateCubic(curvePercent, prevPoint, curPoint, cp[0], cp[1]);
                        shape.Points.Add(new TriPoint(cPos));
                    }
                    break;

                default:
                    // There are at least 3 control points.
                    for(int i = 0; i < cp.Count - 1; i++)
                    {
                        Vector2F midPoint = (cp[i] + cp[i+1]) / 2f;

                        curvePercent = 0f;
                        for (int c = 0; c < pointsPerCurve; c++)
                        {
                            curvePercent += curveIncrement;
                            Vector2F cPos = BezierCurve2D.CalculateQuadratic(curvePercent, prevPoint, midPoint, cp[i]);
                            shape.Points.Add(new TriPoint(cPos));
                        }

                        prevPoint = midPoint;
                    }

                    // Calculate last bezier 
                    curvePercent = 0f;
                    for (int c = 0; c < pointsPerCurve; c++)
                    {
                        curvePercent += curveIncrement;
                        Vector2F cPos = BezierCurve2D.CalculateQuadratic(curvePercent, prevPoint, curPoint, cp[cp.Count - 1]);
                        shape.Points.Add(new TriPoint(cPos));
                    }
                    break;
            }

            cp.Clear();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
