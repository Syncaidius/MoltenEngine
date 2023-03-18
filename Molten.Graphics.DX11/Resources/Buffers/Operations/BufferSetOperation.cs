﻿using Molten.IO;

namespace Molten.Graphics
{
    internal struct BufferSetOperation<T> : IGraphicsResourceTask
        where T : unmanaged
    {
        /// <summary>The number of bytes to offset the change, from the start of the provided <see cref="Segment"/>.</summary>
        internal uint ByteOffset;

        /// <summary>The number of elements to be copied.</summary>
        internal uint ElementCount;

        /// <summary>
        /// Number of bytes per element.
        /// </summary>
        public uint Stride;

        internal uint DataStartIndex;

        /// <summary>The data to be set.</summary>
        internal T[] Data;

        internal BufferDX11 DestBuffer;

        internal Action CompletionCallback;

        internal StagingBuffer Staging;

        public bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            DestBuffer.GetStream(cmd as CommandQueueDX11, ByteOffset, Stride, ElementCount, WriteDataCallback, Staging);
            CompletionCallback?.Invoke();

            return false;
        }

        private void WriteDataCallback(BufferDX11 buffer, RawStream stream)
        {
            stream.WriteRange(Data, DataStartIndex, ElementCount);
        }
    }
}
