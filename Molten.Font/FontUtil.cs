using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public static class FontUtil
    {
        public static readonly DateTime BaseTime = new DateTime(1904, 1, 1, 0, 0, 0);

        /// <summary>Helper for when we're checking if table offsets are meant to be equivilent to C++ NULL (0).</summary>
        public const int NULL = 0;

        public const int POINTS_PER_INCH = 72;

        /// <summary>Gets an offset from the start position to the current position.</summary>
        /// <param name="startPos">The starting position.</param>
        /// <param name="curPos">The current position.</param>
        /// <returns></returns>
        public static long GetOffset(long startPos, long curPos)
        {
            return curPos - startPos;
        }

        /// <summary>Unpacks a 16-bit signed fixed number with the low 14 bits of fraction (2.14), into a float.</summary>
        /// <param name="packed">The packed 16-bit signed value.</param>
        /// <returns></returns>
        public static float FromF2DOT14(int packed)
        {
            return (short)packed / 16384.0f;
        }

        /// <summary>Converts a 32-bit signed integer into a fixed-point float with a 16-bit integral and 16-bit fraction (16.16).</summary>
        /// <param name="fixedValue"></param>
        /// <returns></returns>
        public static float FixedToDouble(int fixedValue)
        {
            int integer = (fixedValue >> 16);
            int fraction = (fixedValue << 16) >> 16;
            float i = fraction / 65536.0f;

            return integer + i;
        }

        /// <summary>Converts a long-date (64 bit times, seconds since 00:00:00, 1-Jan-1904) in to a <see cref="DateTime"/></summary>
        /// <param name="secondsFromBase">The number of seconds since <see cref="BaseTime"/>.</param>
        /// <returns></returns>
        public static DateTime FromLongDate(long secondsFromBase)
        {
            return BaseTime + TimeSpan.FromSeconds(secondsFromBase);
        }

        internal static void TransformGlyph(Glyph glyph, Matrix2x2 matrix)
        {
            RectangleF bounds = RectangleF.Empty;

            GlyphPoint[] glyphPoints = glyph.Points;
            for (int i = glyphPoints.Length - 1; i >= 0; --i)
            {
                GlyphPoint p = glyphPoints[i];

                // Use a Vector2 transform-normal calculation here
                Vector2 pNew = Vector2.TransformNormal(p.Point, matrix);
                glyphPoints[i] = new GlyphPoint(pNew, p.IsOnCurve);

                // Check if transformed point goes outside of the glyph's current bounds.
                if (pNew.X < bounds.X)
                    bounds.X = pNew.X;

                if (pNew.X > bounds.Right)
                    bounds.Right = pNew.X;

                if (pNew.Y < bounds.Y)
                    bounds.Y = pNew.Y;

                if (pNew.Y > bounds.Bottom)
                    bounds.Bottom = pNew.Y;
            }

            glyph.Bounds = (Rectangle)bounds;
        }

        internal static void OffsetGlyph(Glyph glyph, int dx, int dy)
        {
            GlyphPoint[] points = glyph.Points;
            for (int i = points.Length - 1; i >= 0; --i)
                points[i] = new GlyphPoint(points[i].Point + new Vector2(dx, dy), points[i].IsOnCurve);

            // Update bounds
            Rectangle curBounds = glyph.Bounds;
            glyph.Bounds = new Rectangle(
               (curBounds.X + dx),
               (curBounds.Y + dy),
               (curBounds.Right + dx),
               (curBounds.Bottom + dy));
        }

        /// <summary>
        /// Reads a 4-byte tag and converts it into a string.</para>
        /// From MS Docs: Array of four uint8s (length = 32 bits) used to identify a script, language system, feature, or baseline. <para/>
        /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/font-file
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string ReadTag(BinaryEndianAgnosticReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// Creates a new array with the specified number of extra elements, then fills it with the elements from the original array.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="original">The original array from which to create an extended clone.</param>
        /// <param name="lengthToAdd">The number of extra elements to add to the clone's length.</param>
        /// <returns></returns>
        public static T[] CloneAndExtendArray<T>(T[] original, int lengthToAdd = 0)
        {
            T[] cloned = new T[original.Length + lengthToAdd];
            Array.Copy(original, cloned, original.Length);
            return cloned;
        }

        /// <summary>
        /// Convert from a point-unit value to a pixel value
        /// </summary>
        /// <param name="targetPointSize">The target font point size.</param>
        /// <param name="pixelsPerInch">The number of pixels per inch.</param>
        /// <returns></returns>
        public static float FontPointsToPixels(float targetPointSize, int pixelsPerInch = 96)
        {
            // Points = pixels * 72 / 96
            // Pixels = targetPointSize * 96 /72
            // Pixels = targetPointSize * resolution / pointPerInch
            // See: http://stackoverflow.com/questions/139655/convert-pixels-to-points
            return targetPointSize * pixelsPerInch / POINTS_PER_INCH;
        }
    }
}
