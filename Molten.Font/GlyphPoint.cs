using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public struct GlyphPoint
    {
        public Vector2 Point;
        public bool IsOnCurve;

        internal GlyphPoint(float x, float y, bool isOnCurve)
        {
            Point = new Vector2(x, y);
            IsOnCurve = isOnCurve;
        }

        internal GlyphPoint(Vector2 p, bool isOnCurve)
        {
            Point = p;
            IsOnCurve = isOnCurve;
        }

        internal GlyphPoint Offset(short dx, short dy) { return new GlyphPoint(new Vector2(Point.X + dx, Point.Y + dy), IsOnCurve); }

        internal void ApplyScale(float scale)
        {
            Point *= scale;
        }

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

        public float X
        {
            get => Point.X;
            internal set => Point.X = value;
        }

        public float Y
        {
            get => Point.Y;
            internal set => Point.Y = value;
        }
    }
}
