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
        List<Shape> _shapes;
        IReadOnlyList<Shape> _readOnlyShapes;

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

        public IReadOnlyList<Shape> Shapes => _readOnlyShapes;

        internal Glyph(Rectangle bounds, ushort[] contourEndPoints, GlyphPoint[] points, byte[] instructions)
        {
            Bounds = bounds;
            ContourEndPoints = contourEndPoints;
            Instructions = instructions;
            _points = points;
            _shapes = new List<Shape>();
            _readOnlyShapes = _shapes.AsReadOnly();
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
        /// Flips the Y axis of all the glyph's shapes.
        /// </summary>
        public void FlipShapeYAxis()
        {
            foreach(Shape s in _shapes)
            {
                for(int i = 0; i < s.Points.Count; i++)
                    s.Points[i].Y = _bounds.Height - s.Points[i].Y;
            }
        }

        /// <summary>
        /// Populates the <see cref="Shapes"/> list based on the gylph's outline.
        /// </summary>
        /// <param name="flipYAxis">If true, the Y axis of the glyph's points will be flipped.</param>
        /// <returns></returns>
        public void PopulateShapes()
        {
            int start = 0;
            Vector2[] windingPoints = new Vector2[3];
            List<Shape> holes = new List<Shape>();
            GlyphPoint p;

            for (int i = 0; i < ContourEndPoints.Length; i++)
            {
                Shape shape = new Shape();
                int end = ContourEndPoints[i];
                int curWindPoint = 0;

                Winding winding = Winding.Collinear;
                // TODO improve with winding number system: https://en.wikipedia.org/wiki/Winding_number

                for (int j = start; j <= end; j++)
                {
                    p = _points[j];
                    if (p.IsOnCurve)
                    {
                        if (curWindPoint < windingPoints.Length)
                        {
                            windingPoints[curWindPoint++] = p.Point;
                            if (curWindPoint == 3)
                                winding = MathHelper.GetWinding(windingPoints[0], windingPoints[1], windingPoints[2]);
                        }

                        shape.Points.Add(new ShapePoint(p.Point));
                    }
                }

                // Add the first point again to create a loop (for rendering only)
                shape.Points.Add(shape.Points[0]);

                shape.CalculateBounds();
                if (winding == Winding.Clockwise)
                    _shapes.Add(shape);
                else
                    holes.Add(shape);

                start = end + 1;
            }

            // Figure out which holes belong to which shape
            foreach (Shape h in holes)
            {
                foreach (Shape s in _shapes)
                {
                    if (s.Bounds.Contains(h.Bounds))
                    {
                        s.Holes.Add(h);
                        break;
                    }
                }
            }
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
