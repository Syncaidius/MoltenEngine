using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public enum ContentHandleStatus
    {
        NotStarted = 0,

        Processing = 1,

        Completed = 2,

        /// <summary>
        /// The content was unloaded by a <see cref="ContentManager"/>.
        /// </summary>
        Unloaded = 3,
    }
}
