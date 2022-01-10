using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal static class SilkEnumExtensions
    {
        public static bool HasFlag(this FormatSupport value, FormatSupport flag)
        {
            return (value & flag) == flag;
        }

        public static Format ToApi(this GraphicsFormat format)
       {
            return (Format)format;
        }

        public static GraphicsFormat FromApi(this Format format)
        {
            return (GraphicsFormat)format;
        }
    }
}
