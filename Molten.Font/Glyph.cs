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
            Vector2[] windingPoints = new Vector2[3];
            List<Shape> holes = new List<Shape>();
            List<Vector2> cp = new List<Vector2>();
            Vector2 prevCurvePoint = Vector2.Zero;
            int start = 0;
            GlyphPoint p = GlyphPoint.Empty;

            for (int i = 0; i < ContourEndPoints.Length; i++)
            {
                Shape shape = new Shape();
                int end = ContourEndPoints[i];
                int curWindPoint = 0;
                int windingNumber = 0;
                float curveIncrement = 1.0f / pointsPerCurve;
                cp.Clear();

                for (int j = start; j <= end; j++)
                {
                    p = _points[j];

                    // If off curve, it's a bezier control point.
                    if (p.IsOnCurve)
                    {
                        PlotCurve(shape, prevCurvePoint, p.Point, cp, pointsPerCurve, curveIncrement);
                        prevCurvePoint = p.Point;

                        // Use winding number/weight to determine winding: https://en.wikipedia.org/wiki/Winding_number
                        if (curWindPoint < windingPoints.Length)
                        {
                            windingPoints[curWindPoint++] = p.Point;
                            if (curWindPoint == 3)
                                windingNumber += MathHelper.GetWindingSign(windingPoints[0], windingPoints[1], windingPoints[2]);
                        }
                        else
                        {
                            windingPoints[0] = windingPoints[1];
                            windingPoints[1] = windingPoints[2];
                            windingPoints[2] = p.Point;
                            windingNumber += MathHelper.GetWindingSign(windingPoints[0], windingPoints[1], windingPoints[2]);
                        }
                    }
                    else
                    {
                        cp.Add(p.Point);
                    }
                }

                // Close contour, by linking the end point back to the start point.
                if (cp.Count > 0)
                    PlotCurve(shape, prevCurvePoint, (Vector2)shape.Points[0], cp, pointsPerCurve, curveIncrement);

                // Add the first point again to create a loop (for rendering only)
                shape.CalculateBounds();

                // Flip points
                if (flipYAxis)
                {
                    for (int j = 0; j < shape.Points.Count; j++)
                        shape.Points[j].Y = _bounds.Height - shape.Points[j].Y;
                }

                // Add the missing closure point after flipping.
                if(cp.Count == 0)
                    shape.Points.Add(shape.Points[0]);

                if (windingNumber > 0)
                    result.Add(shape);
                else
                    holes.Add(shape);

                start = end + 1;
            }

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

        private void PlotCurve(Shape shape, Vector2 prevPoint, Vector2 curPoint, List<Vector2> cp, float pointsPerCurve, float curveIncrement)
        {
            float curvePercent = 0f;
            switch (cp.Count)
            {
                case 0: // Line
                    shape.Points.Add(new ShapePoint(curPoint));
                    break;

                case 1: // Quadratic bezier curve
                    curvePercent = 0f;
                    for (int c = 0; c < pointsPerCurve; c++)
                    {
                        curvePercent += curveIncrement;
                        Vector2 cPos = BezierCurve2D.CalculateQuadratic(curvePercent, prevPoint, curPoint, cp[0]);
                        shape.Points.Add(new ShapePoint(cPos));
                    }
                    break;

                case 2: // Cubic curve
                    curvePercent = 0f;
                    for (int c = 0; c < pointsPerCurve; c++)
                    {
                        curvePercent += curveIncrement;
                        Vector2 cPos = BezierCurve2D.CalculateCubic(curvePercent, prevPoint, curPoint, cp[0], cp[1]);
                        shape.Points.Add(new ShapePoint(cPos));
                    }
                    break;

                default:
                    // There are at least 3 control points.
                    for(int i = 0; i < cp.Count - 1; i++)
                    {
                        Vector2 midPoint = (cp[i] + cp[i+1]) / 2f;

                        curvePercent = 0f;
                        for (int c = 0; c < pointsPerCurve; c++)
                        {
                            curvePercent += curveIncrement;
                            Vector2 cPos = BezierCurve2D.CalculateQuadratic(curvePercent, prevPoint, midPoint, cp[i]);
                            shape.Points.Add(new ShapePoint(cPos));
                        }

                        prevPoint = midPoint;
                    }

                    // Calculate last bezier 
                    curvePercent = 0f;
                    for (int c = 0; c < pointsPerCurve; c++)
                    {
                        curvePercent += curveIncrement;
                        Vector2 cPos = BezierCurve2D.CalculateQuadratic(curvePercent, prevPoint, curPoint, cp[cp.Count - 1]);
                        shape.Points.Add(new ShapePoint(cPos));
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
