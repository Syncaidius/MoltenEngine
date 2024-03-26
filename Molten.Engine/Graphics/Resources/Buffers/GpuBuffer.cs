namespace Molten.Graphics;

public abstract class GpuBuffer : GpuResource
{
    List<GpuBuffer> _allocations;
    List<GpuBuffer> _freeAllocations;

    /// <summary>
    /// Creates a new instance of <see cref="GpuBuffer"/>.
    /// </summary>
    /// <param name="device">The <see cref="GpuDevice"/> that the buffer is bound to.</param>
    /// <param name="stride">The number of bytes per buffer element, in bytes.</param>
    /// <param name="numElements">The number of elements in the buffer.</param>
    /// <param name="flags">Resource flags which define how the buffer can be used.</param>
    /// <param name="type">The type of buffer.</param>
    /// <param name="alignment">The alignment of the buffer, in bytes.</param>
    protected GpuBuffer(GpuDevice device, uint stride, ulong numElements, GpuResourceFlags flags, GpuBufferType type, uint alignment) :
        base(device, flags)
    {
        _allocations = new List<GpuBuffer>();
        _freeAllocations = new List<GpuBuffer>();
        ResourceFormat = GpuResourceFormat.Unknown;
        BufferType = type;
        Stride = stride;
        ElementCount = numElements;
        SizeInBytes = stride * numElements;
        Alignment = alignment;
    }

    /// <summary>
    /// Allocates a <see cref="GpuBuffer"/> as a sub-buffer within the current <see cref="GpuBuffer"/>.
    /// </summary>
    /// <param name="stride"></param>
    /// <param name="numElements"></param>
    /// <param name="alignment"></param>
    /// <param name="flags"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public GpuBuffer Allocate(uint stride, ulong numElements, GpuResourceFlags flags, GpuBufferType type, uint alignment = 1)
    {
        ulong required = stride * numElements;
        ulong alignedOffset = EngineUtil.Align(Offset + AllocatedBytes, alignment);
        ulong remaining = SizeInBytes - alignedOffset;

        // Check for any free allocations we can re-use. 
        // Choose the smallest-fitting allocation, which may also include a new allocation using the remaining space.
        ulong smallest = remaining; 
        int freeIndex = -1;
        GpuBuffer subBuffer = null;
        for (int i = 0; i < _freeAllocations.Count; i++)
        {
            GpuBuffer alloc = _freeAllocations[i];
            if (alloc.SizeInBytes >= required && alloc.SizeInBytes < smallest)
            {
                smallest = alloc.SizeInBytes;
                freeIndex = i;
                subBuffer = alloc;
            }
        }

        if (subBuffer != null)
        {
            _freeAllocations.RemoveAt(freeIndex);
            return subBuffer;
        }

        // Not enough available space for a new allocation?
        if (remaining < required)
            return null;

        AllocatedBytes = alignedOffset + required;

        subBuffer = OnAllocateSubBuffer(alignedOffset, stride, numElements, flags, type, alignment);
        _allocations.Add(subBuffer);

        return subBuffer;
    }

    /// <summary>
    /// Allocates a <see cref="GpuBuffer"/> as a sub-buffer within the current <see cref="GpuBuffer"/>.
    /// </summary>
    /// <param name="numBytes"></param>
    /// <param name="alignment"></param>
    /// <param name="flags"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public GpuBuffer Allocate(ulong numBytes, GpuResourceFlags flags, GpuBufferType type, uint alignment = 1)
    {
        return Allocate(1, numBytes, flags, type, alignment);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stride"></param>
    /// <param name="numElements"></param>
    /// <param name="alignment"></param>
    /// <returns></returns>
    public GpuBuffer Allocate(uint stride, ulong numElements, uint alignment = 1)
    {
        return Allocate(stride * numElements, Flags, BufferType, alignment);
    }

    /// <summary>
    /// Frees a sub-allocated buffer on the current <see cref="GpuBuffer"/>. If the buffer was not allocated by this buffer, an exception is thrown.
    /// <para>Freeing a sub-allocated buffer will allow it to be reallocated during a future allocate request.</para>
    /// </summary>
    public void Free(GpuBuffer buffer)
    {
        // TODO Implement pooling of sub buffer instances

        if (ParentBuffer == this)
            _freeAllocations.Add(buffer);
        else
            throw new InvalidOperationException("The graphics buffer was not allocated by this buffer.");
    }

    /// <summary>
    /// Frees the current buffer from its <see cref="ParentBuffer"/>. If <see cref="ParentBuffer"/> is null, an exception is thrown.
    /// <para>Freeing a sub-allocated buffer will allow it to be reallocated during a future allocate request.</para>
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Free()
    {
        if(ParentBuffer != null)
            ParentBuffer.Free(this);
        else
            throw new InvalidOperationException("The graphics buffer was not allocated by another buffer.");
    }

    /// <summary>
    /// Sets data on a <see cref="GpuBuffer"/> based on the given <see cref="GpuPriority"/>.
    /// </summary>
    /// <typeparam name="T">The type of data to be set.</typeparam>
    /// <param name="buffer">The <see cref="GpuBuffer"/> to set data.</param>
    /// <param name="priority"></param>
    /// <param name="data"></param>
    /// <param name="discard">Discard the data currently in the buffer and allocate fresh memory for the provided data.</param>
    /// <param name="completeCallback"></param>
    public void SetData<T>(GpuPriority priority, T[] data, bool discard, GpuTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        SetData(priority, data, 0, (uint)data.Length, discard, 0, completeCallback);
    }

    /// <summary>
    /// Sets data on a <see cref="GpuBuffer"/> based on the given <see cref="GpuPriority"/>.
    /// </summary>
    /// <typeparam name="T">The type of data to be set.</typeparam>
    /// <param name="priority"></param>
    /// <param name="data"></param>
    /// <param name="startIndex">The start index within <paramref name="data"/> to copy.</param>
    /// <param name="elementCount"></param>
    /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
    /// <param name="completeCallback"></param>
    /// <param name="discard">If true, the previous data will be discarded. Ignored if not applicable to the current buffer.</param>
    public void SetData<T>(GpuPriority priority, T[] data, ulong startIndex, ulong elementCount, bool discard, uint byteOffset = 0, GpuTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        BufferSetTask<T> op = Device.Tasks.Get<BufferSetTask<T>>();
        op.ByteOffset = byteOffset;
        op.Discard = discard;
        op.ElementCount = elementCount;
        op.Resource = this;
        op.OnCompleted += completeCallback;

        // Only copy the part we need from the source data, starting from startIndex.
        op.Data = new T[data.Length];
        op.DataStartIndex = 0;
        Array.Copy(data, (long)startIndex, op.Data, 0, (long)elementCount);

        Device.Tasks.Push(priority, op);
    }

    internal void SetDataImmediate<T>(GpuCommandList cmd, T[] data, bool discard, ulong byteOffset = 0)
        where T : unmanaged
    {
        SetDataImmediate(cmd, data, 0, (ulong)data.LongLength, discard, byteOffset);
    }

    internal void SetDataImmediate<T>(GpuCommandList cmd, T[] data, uint startIndex, ulong elementCount, bool discard, ulong byteOffset = 0)
        where T : unmanaged
    {
        ulong actualOffset = Offset + byteOffset;
        GpuMapType type = discard ? GpuMapType.Discard : GpuMapType.Write;

        if (Flags.Has(GpuResourceFlags.UploadMemory))
        {
            using (GpuStream stream = cmd.MapResource(this, 0, actualOffset, type))
                stream.WriteRange(data, startIndex, elementCount);
        }
        else
        {
            GpuBuffer staging = cmd.Device.Frame.StagingBuffer;
            using (GpuStream stream = cmd.MapResource(staging, 0, byteOffset, GpuMapType.Write))
                stream.WriteRange(data, startIndex, ElementCount);

            cmd.CopyResource(staging, this);
        }
    }

    /// <summary>Retrieves data from a <see cref="GpuBuffer"/>.</summary>
    /// <param name="priority">The priority of the operation</param>
    /// <param name="destination">The destination array. Must be big enough to contain the retrieved data.</param>
    /// <param name="startIndex">The start index within the destination array at which to place the retrieved data.</param>
    /// <param name="count">The number of elements to retrieve</param>
    /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
    /// <param name="completionCallback">A callback to run once the operation is completed.</param>
    public void GetData<T>(GpuPriority priority, T[] destination, uint startIndex, uint count, ulong byteOffset, Action<T[]> completionCallback = null)
        where T : unmanaged
    {
        if (!Flags.Has(GpuResourceFlags.DownloadMemory))
            throw new GpuResourceException(this, "Cannot use GetData() on a non-readable buffer.");

        if (destination.Length < count)
            throw new ArgumentException("The provided destination array is not large enough.");

        BufferGetTask<T> task = Device.Tasks.Get<BufferGetTask<T>>();
        task.ByteOffset = byteOffset;
        task.Count = count;
        task.DestArray = destination;
        task.DestIndex = startIndex;
        task.OnGetData += completionCallback;
        task.Resource = this;

        Device.Tasks.Push(priority, task);
    }

    /// <summary>Retrieves data from a <see cref="GpuBuffer"/>.</summary>
    /// <param name="cmd">The command list that will immediately process </param>
    /// <param name="destination">The destination array. Must be big enough to contain the retrieved data.</param>
    /// <param name="startIndex">The start index within the destination array at which to place the retrieved data.</param>
    /// <param name="count">The number of elements to retrieve</param>
    /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
    internal void GetDataImmediate<T>(GpuCommandList cmd, T[] destination, uint startIndex, uint count, ulong byteOffset)
        where T : unmanaged
    {
        if (!Flags.Has(GpuResourceFlags.DownloadMemory))
            throw new GpuResourceException(this, "Cannot use GetData() on a non-readable buffer.");

        if (destination.Length < count)
            throw new ArgumentException("The provided destination array is not large enough.");

        ulong actualOffset = Offset + byteOffset;
        // Now set the structured variable's data
        using (GpuStream stream = cmd.MapResource(this, 0, actualOffset, GpuMapType.Read))
            stream.ReadRange(destination, startIndex, count);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="stride"></param>
    /// <param name="numElements"></param>
    /// <param name="flags"></param>
    /// <param name="type"></param>
    /// <param name="alignment"></param>
    /// <returns></returns>
    protected abstract GpuBuffer OnAllocateSubBuffer(ulong offset, uint stride, ulong numElements, GpuResourceFlags flags, GpuBufferType type, uint alignment);

    /// <summary>
    /// Gets the stride (byte size) of each element within the current <see cref="GpuBuffer"/>.
    /// </summary>
    public uint Stride { get; }

    /// <summary>
    /// Gets the number of elements that the current <see cref="GpuBuffer"/> can store.
    /// </summary>
    public ulong ElementCount { get; }

    /// <summary>
    /// Gets the total size of the buffer, in bytes.
    /// </summary>
    public override ulong SizeInBytes { get; protected set; }

    /// <summary>
    /// Gets the type of the current <see cref="GpuBuffer"/>.
    /// </summary>
    public GpuBufferType BufferType { get; }

    /// <summary>
    /// Gets the vertex input layout of the current <see cref="GpuBuffer"/>, if any.
    /// <para>This property is only set if the current <see cref="BufferType"/> is <see cref="GpuBufferType.Vertex"/>.</para>
    /// </summary>
    public ShaderIOLayout VertexLayout { get; internal set; }

    /// <summary>
    /// Gets the total number of bytes that have been sub-allocated by the current <see cref="GpuBuffer"/>.
    /// </summary>
    public ulong AllocatedBytes { get; private set; }

    /// <summary>
    /// Gets the offset of the current <see cref="GpuBuffer"/> within its parent <see cref="GpuBuffer"/>.
    /// <para>If the buffer has no parent, this value should always be 0.</para>
    /// </summary>
    public ulong Offset { get; protected set; }

    /// <summary>
    /// Gets the expected alignment of the current <see cref="GpuBuffer"/>.
    /// </summary>
    public uint Alignment { get; private set; }

    /// <summary>
    /// Gets the parent <see cref="GpuBuffer"/> of the current <see cref="GpuBuffer"/>, if any.
    /// </summary>
    public GpuBuffer ParentBuffer { get; protected set; }

    /// <summary>
    /// Gets a list of all sub-allocated <see cref="GpuBuffer"/> that were allocated by the current <see cref="GpuBuffer"/>.
    /// </summary>
    internal IReadOnlyList<GpuBuffer> Allocations => _allocations;
}
