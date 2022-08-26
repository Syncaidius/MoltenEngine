using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    /// <summary>
    /// Represents the orientation or flow direction of an element.
    /// </summary>
    public enum UIElementFlowDirection
    {
        /// <summary>
        /// The <see cref="UIScrollBar"/> will scroll and render vertically.
        /// </summary>
        Vertical = 0,

        /// <summary>
        /// The <see cref="UIScrollBar"/> will scroll and render horizontally.
        /// </summary>
        Horizontal = 1
    }
}
