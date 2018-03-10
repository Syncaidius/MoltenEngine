using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public struct GlyphPoint
    {
        public static readonly GlyphPoint Empty = new GlyphPoint();
        public Vector2F Point;
        public bool IsOnCurve;

        internal GlyphPoint(float x, float y, bool isOnCurve)
        {
            Point = new Vector2F(x, y);
            IsOnCurve = isOnCurve;
        }

        internal GlyphPoint(Vector2F p, bool isOnCurve)
        {
            Point = p;
            IsOnCurve = isOnCurve;
        }

        /// <summary>
        /// Offsets the current glyh by the specified amount.
        /// </summary>
        /// <param name="offsetX">The amount to offset along the X axis.</param>
        /// <param name="offsetY">The amount to offset along the Y axis.</param>
        /// <returns></returns>
        internal GlyphPoint Offset(short offsetX, short offsetY)
        {
            return new GlyphPoint(new Vector2F(Point.X + offsetX, Point.Y + offsetY), IsOnCurve);
        }

        /// <summary>
        /// Applies the specified scale to the X and Y axis of the current <see cref="GlyphPoint"/>.
        /// </summary>
        /// <param name="scale">The scale value.</param>
        internal void ApplyScale(float scale)
        {
            Point *= scale;
        }

        /// <summary>
        /// Applies the specified scale to only the X axis of the current <see cref="GlyphPoint"/>.
        /// </summary>
        /// <param name="scale">The scale value.</param>
        internal void ApplyScaleOnlyOnXAxis(float scale)
        {
            Point.X *= scale;
        }

        /// <summary>
        /// Applies an offset to the X element of the current <see cref="GlyphPoint"/>.
        /// </summary>
        /// <param name="offsetX">The X offset to apply.</param>
        internal void OffsetX(float offsetX) => Point.X += offsetX;

        /// <summary>
        /// Applies an offset to the X element of the current <see cref="GlyphPoint"/>.
        /// </summary>
        /// <param name="offsetX">The X offset to apply.</param>
        internal void OffsetY(float offsetY) => Point.Y += offsetY;

        public override string ToString()
        {
            return $"X: {Point.X}, Y: {Point.Y}, OnCurve: {IsOnCurve}";
        }

        /// <summary>
        /// Gets the X value of the current <see cref="GlyphPoint"/>.
        /// </summary>
        public float X
        {
            get => Point.X;
            internal set => Point.X = value;
        }

        /// <summary>
        /// Gets the X value of the current <see cref="GlyphPoint"/>.
        /// </summary>
        public float Y
        {
            get => Point.Y;
            internal set => Point.Y = value;
        }
    }
}
