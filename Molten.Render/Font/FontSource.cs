using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum FontSource
    {
        /// <summary>The font is located at the provided path.</summary>
        Path = 0,

        /// <summary>The font is located in the system's installed font collection.</summary>
        System = 1,
    }
}
