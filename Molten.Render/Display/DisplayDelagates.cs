using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A delegate representing a display output change.</summary>
    /// <param name="adapter">The adapter.</param>
    /// <param name="output">The output.</param>
    public delegate void DisplayOutputChanged(IDisplayOutput output);
}
