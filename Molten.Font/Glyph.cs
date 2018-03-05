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

        public GlyphPoint[] Points { get; private set; }

        public byte[] Instructions { get; private set; }

        internal Glyph(Rectangle bounds, ushort[] contourEndPoints, GlyphPoint[] points, byte[] instructions)
        {
            Bounds = bounds;
            ContourEndPoints = contourEndPoints;
            Instructions = instructions;
            Points = points;
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

            Points = ArrayHelper.Concat(Points, other.Points);
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

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
