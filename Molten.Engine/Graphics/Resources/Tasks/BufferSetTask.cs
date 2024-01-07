namespace Molten.Graphics;

internal class BufferSetTask<T> : GraphicsResourceTask<GraphicsBuffer>
    where T : unmanaged
{
    /// <summary>The number of bytes to offset the change, from the start of the provided <see cref="Segment"/>.</summary>
    internal uint ByteOffset;

    /// <summary>The number of elements to be copied.</summary>
    internal uint ElementCount;

    internal GraphicsMapType MapType;

    internal uint DataStartIndex;

    /// <summary>The data to be set.</summary>
    internal T[] Data;

    internal GraphicsBuffer DestBuffer;

    internal Action CompletionCallback;

    public override void ClearForPool()
    {
        ByteOffset = 0;
        ElementCount = 0;
        MapType = GraphicsMapType.Read;
        DataStartIndex = 0;
        Data = null;
        DestBuffer = null;
        CompletionCallback = null;
    }

    protected override bool OnProcess(GraphicsQueue queue)
    {
        if (Resource.Flags.Has(GraphicsResourceFlags.CpuWrite))
        {
            using (GraphicsStream stream = queue.MapResource(Resource, 0, ByteOffset, MapType))
                stream.WriteRange(Data, DataStartIndex, ElementCount);
        }
        else
        {
            GraphicsBuffer staging = queue.Device.Frame.StagingBuffer;
            using (GraphicsStream stream = queue.MapResource(staging, 0, ByteOffset, GraphicsMapType.Write))
                stream.WriteRange(Data, DataStartIndex, ElementCount);

            queue.CopyResource(staging, Resource);
        }

        CompletionCallback?.Invoke();
        return false;
    }

    public override void Validate()
    {
        throw new NotImplementedException();
    }
}
