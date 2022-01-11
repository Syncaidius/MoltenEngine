using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal static class SilkDxgiExtensions
    {
        public static Rectangle<int> ToApi(this Rectangle r)
        {
            return new Rectangle<int>(r.X, r.Y, r.Width, r.Height);
        }

        public static Rectangle FromApi(this Rectangle<int> rect)
        {
            return new Rectangle(rect.Origin.X, rect.Origin.Y, rect.Size.X, rect.Size.Y);
        }
    }
}
