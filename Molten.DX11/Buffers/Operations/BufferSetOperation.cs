﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct BufferSetOperation<T> : IBufferOperation
        where T : struct
    {
        /// <summary>The number of bytes to offset the change, from the start of the provided <see cref="Segment"/>.</summary>
        public int ByteOffset;

        /// <summary>The number of bytes per element in <see cref="Data"/>.</summary>
        public int DataStride;

        /// <summary>The number of elements to be copied.</summary>
        public int Count;

        internal int StartIndex;

        /// <summary>The data to be set.</summary>
        public T[] Data;

        public BufferSegment DestinationSegment;

        internal Action CompletionCallback;

        internal StagingBuffer Staging;

        public void Process(PipeDX11 pipe)
        {
            DestinationSegment.Parent.Set<T>(pipe, Data, StartIndex, Count, DataStride, ByteOffset, Staging);
            CompletionCallback?.Invoke();
        }
    }
}
