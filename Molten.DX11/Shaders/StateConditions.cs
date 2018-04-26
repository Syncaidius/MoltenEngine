using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [Flags]
    public enum StateConditions : byte
    {
        /// <summary>
        /// No conditions.
        /// </summary>
        None = 0,

        Multisampling = 1,

        AnisotropicFiltering = 1 << 1,

        All = Multisampling | AnisotropicFiltering,
    }
}
