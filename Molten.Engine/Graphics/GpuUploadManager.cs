using Molten.Collections;

namespace Molten.Graphics;

/// <summary>
/// Manages per-frame staging/upload buffers.
/// </summary>
internal class GpuUploadManager
{
    class Frame : GpuObject
    {
        internal ThreadedList<GpuBuffer> Buffers = new();

        public Frame(GpuDevice device) : base(device) { }    

        protected override void OnGraphicsRelease()
        {
            for(int i = 0; i < Buffers.Count; i++)
                Buffers[i].Dispose(true);
        }
    }

    GpuFrameBuffer<Frame> _frames;
    GpuDevice _device;
    ulong _minAllocation;
    Frame _curFrame;

    /// <summary>
    /// Creates a new instance of <see cref="GpuUploadManager"/>.
    /// </summary>
    /// <param name="device">The <see cref="GpuDevice"/> that the upload manager is bound to.</param>
    /// <param name="minAllocation">The minimum upload buffer size, in bytes</param>
    internal GpuUploadManager(GpuDevice device, ulong minAllocation)
    {
        _minAllocation = minAllocation;
        _device = device;
        _frames = new GpuFrameBuffer<Frame>(device, (gpu) => new Frame(gpu));
    }

    internal void Prepare()
    {
        _curFrame = _frames.Prepare();

        // TODO If there is more than 1 buffer for the new frame, consolidate them all into a single, larger buffer.
    }

    public GpuBuffer Allocate(uint numBytes, uint alignment)
    {
        GpuBuffer buffer = null;

        _curFrame.Buffers.For(0, (index, b) =>
        {
            buffer = b.Allocate(1, numBytes, GpuResourceFlags.UploadMemory, GpuBufferType.Unknown, alignment);
            return buffer != null;
        });

        // Create a new staging buffer.
        if(buffer == null)
        {
            ulong uploadBufferBytes = Math.Max(_minAllocation, numBytes);
            GpuBuffer upBuffer = _device.Resources.CreateUploadBuffer(uploadBufferBytes, alignment);

            _curFrame.Buffers.Add(buffer);
            buffer = upBuffer.Allocate(1, numBytes, GpuResourceFlags.UploadMemory, GpuBufferType.Unknown, alignment);
            if (buffer == null)
                throw new InvalidOperationException("Failed to allocate a new upload buffer space.");
        }

        return buffer;
    }
}
