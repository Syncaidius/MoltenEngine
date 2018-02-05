using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [Flags]
    public enum ObjectUpdateFlags
    {
        /// <summary>The object will not update anything.</summary>
        None = 0,

        /// <summary>The object will update itself.</summary>
        Self = 1,

        /// <summary>The object will update it's children.</summary>
        Children = 2,
    }
}
