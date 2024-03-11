namespace Molten.Graphics;
public partial class GpuCommandList
{
    /// <summary>
    /// Sets data on a <see cref="GpuBuffer"/> based on the given <see cref="GpuPriority"/>.
    /// </summary>
    /// <typeparam name="T">The type of data to be set.</typeparam>
    /// <param name="buffer">The <see cref="GpuBuffer"/> to set data.</param>
    /// <param name="priority"></param>
    /// <param name="data"></param>
    /// <param name="discard">Discard the data currently in the buffer and allocate fresh memory for the provided data.</param>
    /// <param name="completeCallback"></param>
    public void SetData<T>(GpuBuffer buffer, GpuPriority priority, T[] data, bool discard, GraphicsTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        SetData(buffer, priority, data, 0, (uint)data.Length, discard, 0, completeCallback);
    }

    /// <summary>
    /// Sets data on a <see cref="GpuBuffer"/> based on the given <see cref="GpuPriority"/>.
    /// </summary>
    /// <typeparam name="T">The type of data to be set.</typeparam>
    /// <param name="buffer">The <see cref="GpuBuffer"/> to set data.</param>
    /// <param name="priority"></param>
    /// <param name="data"></param>
    /// <param name="startIndex">The start index within <paramref name="data"/> to copy.</param>
    /// <param name="elementCount"></param>
    /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
    /// <param name="completeCallback"></param>
    /// <param name="discard">If true, the previous data will be discarded. Ignored if not applicable to the current buffer.</param>
    public void SetData<T>(GpuBuffer buffer, GpuPriority priority, T[] data, uint startIndex, uint elementCount, bool discard, uint byteOffset = 0,
        GraphicsTask.EventHandler completeCallback = null)
        where T : unmanaged
    {
        BufferSetTask<T> op = Device.Tasks.Get<BufferSetTask<T>>();
        op.ByteOffset = buffer.Offset + byteOffset;
        op.OnCompleted += completeCallback;
        op.MapType = discard ? GpuMapType.Discard : GpuMapType.Write;
        op.ElementCount = elementCount;

        // Custom handling of immediate command, so that we potentially avoid a data copy.
        if (priority == GpuPriority.Immediate)
        {
            op.Data = data;
            op.DataStartIndex = startIndex;
        }
        else
        {
            // Only copy the part we need from the source data, starting from startIndex.
            op.Data = new T[data.Length];
            op.DataStartIndex = 0;
            Array.Copy(data, (int)startIndex, op.Data, 0, elementCount);
        }

        Device.Tasks.Push(this, priority, buffer, op);
    }

    /// <summary>Retrieves data from a <see cref="GpuBuffer"/>.</summary>
    /// <param name="buffer">The buffer from which to get data.</param>
    /// <param name="priority">The priority of the operation</param>
    /// <param name="destination">The destination array. Must be big enough to contain the retrieved data.</param>
    /// <param name="startIndex">The start index within the destination array at which to place the retrieved data.</param>
    /// <param name="count">The number of elements to retrieve</param>
    /// <param name="byteOffset">The start location within the buffer to start copying from, in bytes.</param>
    /// <param name="completionCallback">A callback to run once the operation is completed.</param>
    public void GetData<T>(GpuBuffer buffer, GpuPriority priority, T[] destination, uint startIndex, uint count, uint byteOffset, Action<T[]> completionCallback = null)
        where T : unmanaged
    {
        if (!buffer.Flags.Has(GpuResourceFlags.CpuRead))
            throw new GpuResourceException(buffer, "Cannot use GetData() on a non-readable buffer.");

        if (destination.Length < count)
            throw new ArgumentException("The provided destination array is not large enough.");

        BufferGetTask<T> task = Device.Tasks.Get<BufferGetTask<T>>();
        task.ByteOffset = buffer.Offset + byteOffset;
        task.Count = count;
        task.DestArray = destination;
        task.DestIndex = startIndex;
        task.MapType = GpuMapType.Read;
        task.OnGetData += completionCallback;
        Device.Tasks.Push(this, priority, buffer, task);
    }
}
