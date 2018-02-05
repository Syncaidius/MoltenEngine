using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public enum BufferMode
    {
        /// <summary>The buffer cannot be written to, or read from by the CPU.
        /// Once the first set of data has been set, static buffers update their data by recreating the internal buffer. 
        /// This is an extremely expensive operation.
        /// To avoid this cost, use a staging buffer to copy data into buffers created with this mode.</summary>
        Default = 0,

        /// <summary>The buffer cannot be read or written to by the GPU (or written to by the GPU) after creation. Immutable buffers
        /// perform faster than all other types on the GPU and provide an opportunity for the GPU drivers to optimize 
        /// access/draw/threaded calls.
        /// 
        /// Once the first set of data has been set, immutable buffers update their data by recreating the internal buffer. 
        /// This is an extremely expensive operation.</summary>
        Immutable = 1,

        /// <summary>A dynamic buffer can be written to (but not read from) by the CPU. Useful for data that will change every frame.</summary>
        Dynamic = 2,
    }
}
