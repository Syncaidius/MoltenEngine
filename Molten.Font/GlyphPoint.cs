using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class GlyphPoint
    {
        Vector2 _point;

        internal GlyphPoint(float x, float y, bool isOnCurve)
        {
            _point = new Vector2(x, y);
            IsOnCurve = isOnCurve;
        }

        internal GlyphPoint(Vector2 p, bool isOnCurve)
        {
            _point = p;
            IsOnCurve = isOnCurve;
        }

        public override string ToString()
        {
            return $"X: {_point.X}, Y: {_point.Y}, OnCurve: {IsOnCurve}";
        }

        public Vector2 Coordinate
        {
            get => _point;
            internal set => _point = value;
        }

        public bool IsOnCurve { get; internal set; }

        public float X
        {
            get => _point.X;
            internal set => _point.X = value;
        }

        public float Y
        {
            get => _point.Y;
            internal set => _point.Y = value;
        }
    }
}
