using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public enum InputActionType
    {
        /// <summary>
        /// No tap/click/press.
        /// </summary>
        None = 0,

        /// <summary>
        /// Single tap/click/press.
        /// </summary>
        Single = 1,

        /// <summary>
        /// Double tap/click/press.
        /// </summary>
        Double = 2,
    }
}
