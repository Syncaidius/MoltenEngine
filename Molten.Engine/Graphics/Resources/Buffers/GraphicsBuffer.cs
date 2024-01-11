namespace Molten.Graphics;

public abstract class GraphicsBuffer : GraphicsResource
{
    List<GraphicsBuffer> _allocations;
    List<GraphicsBuffer> _freeAllocations;

    protected GraphicsBuffer(GraphicsDevice device, uint stride, ulong numElements, GraphicsResourceFlags flags, GraphicsBufferType type, uint alignment) :
        base(device, flags)
    {
        _allocations = new List<GraphicsBuffer>();
        _freeAllocations = new List<GraphicsBuffer>();
        ResourceFormat = GraphicsFormat.Unknown;
        BufferType = type;
        Stride = stride;
        ElementCount = numElements;
        SizeInBytes = stride * numElements;
        Alignment = alignment;
    }

    /// <summary>
    /// Allocates a <see cref="GraphicsBuffer"/> as a sub-buffer within the current <see cref="GraphicsBuffer"/>.
    /// </summary>
    /// <param name="stride"></param>
    /// <param name="numElements"></param>
    /// <param name="alignment"></param>
    /// <param name="flags"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public GraphicsBuffer Allocate(uint stride, ulong numElements, GraphicsResourceFlags flags, GraphicsBufferType type, uint alignment = 1)
    {
        ulong required = stride * numElements;
        ulong alignedOffset = EngineUtil.Align(Offset + AllocatedBytes, alignment);
        ulong remaining = SizeInBytes - alignedOffset;

        // Check for any free allocations we can re-use. 
        // Choose the smallest-fitting allocation, which may also include a new allocation using the remaining space.
        ulong smallest = remaining; 
        int freeIndex = -1;
        GraphicsBuffer subBuffer = null;
        for (int i = 0; i < _freeAllocations.Count; i++)
        {
            GraphicsBuffer alloc = _freeAllocations[i];
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
    /// Allocates a <see cref="GraphicsBuffer"/> as a sub-buffer within the current <see cref="GraphicsBuffer"/>.
    /// </summary>
    /// <param name="numBytes"></param>
    /// <param name="alignment"></param>
    /// <param name="flags"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public GraphicsBuffer Allocate(ulong numBytes, GraphicsResourceFlags flags, GraphicsBufferType type, uint alignment = 1)
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
    public GraphicsBuffer Allocate(uint stride, ulong numElements, uint alignment = 1)
    {
        return Allocate(stride * numElements, Flags, BufferType, alignment);
    }

    /// <summary>
    /// Frees a sub-allocated buffer on the current <see cref="GraphicsBuffer"/>. If the buffer was not allocated by this buffer, an exception is thrown.
    /// <para>Freeing a sub-allocated buffer will allow it to be reallocated during a future allocate request.</para>
    /// </summary>
    public void Free(GraphicsBuffer buffer)
    {
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
    /// 
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="stride"></param>
    /// <param name="numElements"></param>
    /// <param name="flags"></param>
    /// <param name="type"></param>
    /// <param name="alignment"></param>
    /// <returns></returns>
    protected abstract GraphicsBuffer OnAllocateSubBuffer(ulong offset, uint stride, ulong numElements, GraphicsResourceFlags flags, GraphicsBufferType type, uint alignment);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="priority"></param>
    /// <param name="data"></param>
    /// <param name="discard">Discard the data currently in the buffer and allocate fresh memory for the provided data.</param>
    /// <param name="completeCallback"></param>
    public void SetData<T>(GraphicsPriority priority, T[] data, bool discard, GraphicsTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        SetData(priority, data, 0, (uint)data.Length, discard, 0, completeCallback);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="priority"></param>
    /// <param name="data"></param>
    /// <param name="startIndex">The start index within <paramref name="data"/> to copy.</param>
    /// <param name="elementCount"></param>
    /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
    /// <param name="completeCallback"></param>
    /// <param name="discard">If true, the previous data will be discarded. Ignored if not applicable to the current buffer.</param>
    public void SetData<T>(GraphicsPriority priority, T[] data, uint startIndex, uint elementCount, bool discard, uint byteOffset = 0, 
        GraphicsTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        BufferSetTask<T> op = Device.Tasks.Get<BufferSetTask<T>>();
        op.ByteOffset = Offset + byteOffset;
        op.OnCompleted += completeCallback;
        op.DestBuffer = this;
        op.MapType = discard ? GraphicsMapType.Discard : GraphicsMapType.Write;
        op.ElementCount = elementCount;

        // Custom handling of immediate command, so that we potentially avoid a data copy.
        if (priority == GraphicsPriority.Immediate)
        {
            op.Data = data;
            op.DataStartIndex = startIndex;
            op.Resource = this;
            op.Process(Device.Queue);
        }
        else
        {
            // Only copy the part we need from the source data, starting from startIndex.
            op.Data = new T[data.Length];
            op.DataStartIndex = 0;
            Array.Copy(data, (int)startIndex, op.Data, 0, elementCount);
            Device.Tasks.Push(priority, this, op);
        }
    }

    /// <summary>Retrieves data from a <see cref="GraphicsBuffer"/>.</summary>
    /// <param name="priority">The priority of the operation</param>
    /// <param name="destination">The destination array. Must be big enough to contain the retrieved data.</param>
    /// <param name="startIndex">The start index within the destination array at which to place the retrieved data.</param>
    /// <param name="count">The number of elements to retrieve</param>
    /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
    /// <param name="completionCallback">A callback to run once the operation is completed.</param>
    public void GetData<T>(GraphicsPriority priority, T[] destination, uint startIndex, uint count, uint byteOffset, Action<T[]> completionCallback = null)
        where T : unmanaged
    {
        if (!Flags.Has(GraphicsResourceFlags.CpuRead))
            throw new GraphicsResourceException(this, "Cannot use GetData() on a non-readable buffer.");

        if (destination.Length < count)
            throw new ArgumentException("The provided destination array is not large enough.");

        BufferGetTask<T> task = Device.Tasks.Get<BufferGetTask<T>>();
        task.ByteOffset = Offset + byteOffset;
        task.Count = count;
        task.DestArray = destination;
        task.DestIndex = startIndex;
        task.MapType = GraphicsMapType.Read;
        task.OnGetData += completionCallback;
        Device.Tasks.Push(priority, this, task);
    }

    /// <summary>
    /// Gets the stride (byte size) of each element within the current <see cref="GraphicsBuffer"/>.
    /// </summary>
    public uint Stride { get; }

    /// <summary>
    /// Gets the number of elements that the current <see cref="GraphicsBuffer"/> can store.
    /// </summary>
    public ulong ElementCount { get; }

    /// <summary>
    /// Gets the total size of the buffer, in bytes.
    /// </summary>
    public override ulong SizeInBytes { get; protected set; }

    /// <summary>
    /// Gets the type of the current <see cref="GraphicsBuffer"/>.
    /// </summary>
    public GraphicsBufferType BufferType { get; }

    /// <summary>
    /// Gets the vertex format of the current <see cref="GraphicsBuffer"/>, if any.
    /// <para>This property is only set if the current <see cref="BufferType"/> is <see cref="GraphicsBufferType.Vertex"/>.</para>
    /// </summary>
    public VertexFormat VertexFormat { get; internal set; }

    /// <summary>
    /// Gets the total number of bytes that have been sub-allocated by the current <see cref="GraphicsBuffer"/>.
    /// </summary>
    public ulong AllocatedBytes { get; private set; }

    /// <summary>
    /// Gets the offset of the current <see cref="GraphicsBuffer"/> within its parent <see cref="GraphicsBuffer"/>.
    /// <para>If the buffer has no parent, this value should always be 0.</para>
    /// </summary>
    public ulong Offset { get; protected set; }

    /// <summary>
    /// Gets the expected alignment of the current <see cref="GraphicsBuffer"/>.
    /// </summary>
    public uint Alignment { get; private set; }

    /// <summary>
    /// Gets the parent <see cref="GraphicsBuffer"/> of the current <see cref="GraphicsBuffer"/>, if any.
    /// </summary>
    public GraphicsBuffer ParentBuffer { get; protected set; }

    /// <summary>
    /// Gets a list of all sub-allocated <see cref="GraphicsBuffer"/> that were allocated by the current <see cref="GraphicsBuffer"/>.
    /// </summary>
    internal IReadOnlyList<GraphicsBuffer> Allocations => _allocations;
}
