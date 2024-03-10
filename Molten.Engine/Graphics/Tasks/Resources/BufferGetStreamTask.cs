namespace Molten.Graphics;

internal class BufferGetStreamTask : GraphicsResourceTask<GraphicsBuffer>
{
    internal uint ByteOffset;

    internal GpuMapType MapType;

    internal GraphicsBuffer Staging;

    /// <summary>A callback to interact with the retrieved stream.</summary>
    internal event Action<GraphicsBuffer, GpuStream> OnStreamOpened;

    public override void ClearForPool()
    {
        Staging = null;
        OnStreamOpened = null;
    }

    public override bool Validate()
    {
        if(MapType.Has(GpuMapType.Read) && !Resource.Flags.Has(GpuResourceFlags.CpuRead))
            throw new GpuResourceException(Resource, "The resource must have CPU read access for reading the mapped data.");

        if (MapType.Has(GpuMapType.Write) && !Resource.Flags.Has(GpuResourceFlags.CpuWrite))
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
