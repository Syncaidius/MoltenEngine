using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct BufferGetOperation<T> : IBufferOperation where T : struct
    {
        /// <summary>The number of bytes to offset the change, from the start of the provided <see cref="SourceSegment"/>.</summary>
        internal int ByteOffset;

        /// <summary>The number of bytes per element in <see cref="Data"/>.</summary>
        internal int DataStride;

        /// <summary>The number of elements to be copied.</summary>
        internal int Count;

        /// <summary>The first index at which to start placing the retrieved data within <see cref="DestinationArray"/>.</summary>
        internal int DestinationIndex;

        internal BufferSegment SourceSegment;

        /// <summary>A callback to send the retrieved data to.</summary>
        internal Action<T[]> CompletionCallback;

        /// <summary>The destination array to store the retrieved data.</summary>
        internal T[] DestinationArray;

        public void Process(PipeDX11 pipe)
        {
            DestinationArray = DestinationArray ?? new T[Count];
            SourceSegment.Parent.Get<T>(pipe, DestinationArray, 0, ByteOffset, DataStride, Count);

            CompletionCallback.Invoke(DestinationArray);
        }
    }
}
