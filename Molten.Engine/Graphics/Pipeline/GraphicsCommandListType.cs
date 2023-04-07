using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum GraphicsCommandListType
    {
        /// <summary>
        /// A command list that is intended to for use over the span of a single frame.
        /// </summary>
        Frame = 0,

        /// <summary>
        /// A command list intended for short, successive submissions within the same frame.
        /// </summary>
        Short = 1,
    }
}
