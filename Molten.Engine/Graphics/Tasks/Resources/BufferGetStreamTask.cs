namespace Molten.Graphics;

internal class BufferGetStreamTask : GpuResourceTask<GpuBuffer>
{
    internal uint ByteOffset;

    internal GpuMapType MapType;

    internal GpuBuffer Staging;

    /// <summary>A callback to interact with the retrieved stream.</summary>
    internal event Action<GpuBuffer, GpuStream> OnStreamOpened;

    public override void ClearForPool()
    {
        Staging = null;
        OnStreamOpened = null;
    }

    public override bool Validate()
    {
        if(MapType == GpuMapType.Read && !Resource.Flags.IsCpuReadable())
            throw new GpuResourceException(Resource, "The resource must have CPU read access for reading the mapped data.");

        if (MapType == GpuMapType.Write && !Resource.Flags.IsCpuWritable())
            throw new GpuResourceException(Resource, "The resource must have CPU write access for writing the mapped data.");

        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        using (GpuStream stream = cmd.MapResource(Resource, 0, ByteOffset, MapType))
            OnStreamOpened?.Invoke(Resource, stream);

        return true;
    }
}
