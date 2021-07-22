using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal static class OpenTKInterop
    {
        public static Rectangle FromApi(this System.Drawing.Rectangle rect)
        {
            return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static System.Drawing.Rectangle ToApi(this Rectangle mRect)
        {
            return new System.Drawing.Rectangle(mRect.X, mRect.Y, mRect.Width, mRect.Height);
        }
    }
}
