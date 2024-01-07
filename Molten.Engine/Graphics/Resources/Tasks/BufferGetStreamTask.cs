namespace Molten.Graphics;

internal class BufferGetStreamTask : GraphicsResourceTask<GraphicsBuffer>
{
    internal uint ByteOffset;

    internal GraphicsMapType MapType;

    internal GraphicsBuffer Staging;

    /// <summary>A callback to interact with the retrieved stream.</summary>
    internal Action<GraphicsBuffer, GraphicsStream> StreamCallback;

    public override void ClearForPool()
    {
        Staging = null;
        StreamCallback = null;
    }

    public override void Validate()
    {
        if(MapType.Has(GraphicsMapType.Read) && !Resource.Flags.Has(GraphicsResourceFlags.CpuRead))
            throw new GraphicsResourceException(Resource, "The resource must have CPU read access for reading the mapped data.");

        if (MapType.Has(GraphicsMapType.Write) && !Resource.Flags.Has(GraphicsResourceFlags.CpuWrite))
            throw new GraphicsResourceException(Resource, "The resource must have CPU write access for writing the mapped data.");
    }

    protected override bool OnProcess(GraphicsQueue queue)
    {
        using (GraphicsStream stream = queue.MapResource(Resource, 0, ByteOffset, MapType))
            StreamCallback?.Invoke(Resource, stream);

        return false;
    }
}
