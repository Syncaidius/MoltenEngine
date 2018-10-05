using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    [Flags]
    public enum StagingBufferFlags
    {
        None = 0,

        /// <summary>A staged vertex buffer can be written to by the CPU via an internal staging buffer. It is then copied to the main buffer. 
        /// This is useful for data that changes less than once per frame. The GPU can read this type of buffer as fast as a default,
        /// but altering buffer data comes at a slight cost to performance.</summary>
        Write = 1,

        /// <summary>The buffer can be read from by the CPU via an internal staging buffer.</summary>
        Read = 2,
    }
}
