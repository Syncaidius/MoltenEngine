using Silk.NET.Direct3D11;
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
    }
}
