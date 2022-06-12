using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public enum UIElementState
    {
        Default = 0,

        Pressed = 1,

        Hovered = 2,

        Disabled = 3,

        /// <summary>
        /// Active, checked or selected.
        /// </summary>
        Active = 4,
    }
}
