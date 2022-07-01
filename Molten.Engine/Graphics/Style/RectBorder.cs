using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public struct RectBorderThickness
    {
        public float Left;

        public float Top;

        public float Right;

        public float Bottom;

        public RectBorderThickness(float value)
        {
            Left = Top = Right = Bottom = value;
        }

        public RectBorderThickness(float leftRight, float topBottom)
        {
            Left = Right = leftRight;
            Top = Bottom = topBottom;
        }

        public RectBorderThickness(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public void Zero()
        {
            Left = Top = Right = Bottom = 0;
        }
    }
}
