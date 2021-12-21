using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [Flags]
    public enum PipeBindTypeFlags
    {
        /// <summary>
        /// No bind type.
        /// </summary>
        None = 0,

        /// <summary>
        /// An input binding.
        /// </summary>
        Input = 1,

        /// <summary>
        /// An output binding.
        /// </summary>
        Output = 2,
    }
}
