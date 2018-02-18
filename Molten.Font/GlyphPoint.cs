using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class GlyphPoint
    {
        Double2 _point;

        internal GlyphPoint(double x, double y, bool isOnCurve)
        {
            _point = new Double2(x, y);
            IsOnCurve = true;
        }

        public Double2 Coordinate
        {
            get => _point;
            internal set => _point = value;
        }

        public bool IsOnCurve { get; internal set; }

        public double X
        {
            get => _point.X;
            internal set => _point.X = value;
        }

        public double Y
        {
            get => _point.Y;
            internal set => _point.Y = value;
        }
    }
}
