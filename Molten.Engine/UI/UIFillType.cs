using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    /// <summary>
    /// Represents the fill type of an element or part.
    /// </summary>
    public enum UIFillType
    {
        /// <summary>
        /// The object will be stretched to fit the bounds of its parent.
        /// </summary>
        Fit = 0,

        /// <summary>
        /// The object will be centered at the middle of its parent, without being stretched or deformed to fit
        /// </summary>
        Center = 1,
    }
}
