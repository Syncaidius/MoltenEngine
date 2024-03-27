using Molten.Collections;

namespace Molten.Graphics;

public class GpuDiscardBuffer<T> : GpuObject
    where T : unmanaged
{
    class Frame : GpuObject
    {
        internal ThreadedList<GpuBuffer> Buffers = new();

        public Frame(GpuDevice device) : base(device) { }

        protected override void OnGraphicsRelease()
        {
            for (int i = 0; i < Buffers.Count; i++)
                Buffers[i].Dispose(true);
        }
    }

    GpuFrameBuffer<Frame> _frames;
    ulong _minAllocation;
    Frame _curFrame;
    uint _stride;

    internal unsafe GpuDiscardBuffer(GpuDevice device, GpuBufferType bufferType, GpuResourceFlags flags, GpuResourceFormat format, ulong initialCapacity) : 
        base(device)
    {
        BufferType = bufferType;
        Flags = flags;
        Format = format;
        _stride = (uint)sizeof(T);

        throw new NotImplementedException("Intialize buffer with the provided initial capacity");
    }

    public GpuBuffer Allocate(uint numElements, uint alignment)
    {
        GpuBuffer buffer = null;

        _curFrame.Buffers.For(0, (index, b) =>
        {
            buffer = b.Allocate(_stride, numElements, GpuResourceFlags.UploadMemory, GpuBufferType.Unknown, alignment);
            return buffer != null;
        });

        // Create a new staging buffer.
        if (buffer == null)
        {
            ulong uploadBufferBytes = Math.Max(_minAllocation, numElements);
            GpuBuffer upBuffer = Device.Resources.CreateBuffer<T>(BufferType, Flags, Format, uploadBufferBytes, alignment);

            _curFrame.Buffers.Add(buffer);
            buffer = upBuffer.Allocate(_stride, numElements, GpuResourceFlags.UploadMemory, GpuBufferType.Unknown, alignment);
            if (buffer == null)
                throw new InvalidOperationException("Failed to allocate a new upload buffer space.");
        }

        return buffer;
    }

    internal void Prepare()
    {
        _curFrame = _frames.Prepare();

        // TODO If there is more than 1 buffer for the new frame, consolidate them all into a single, larger buffer.
    }

    protected override void OnGraphicsRelease()
    {
        _frames.Dispose(true);
    }

    public GpuBufferType BufferType { get; }

    public GpuResourceFlags Flags { get; }

    public GpuResourceFormat Format { get; }
}
