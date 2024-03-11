namespace Molten.Graphics;

internal class BufferSetTask<T> : GraphicsResourceTask<GpuBuffer>
    where T : unmanaged
{
    /// <summary>The number of bytes to offset the change, from the start of the provided <see cref="Segment"/>.</summary>
    internal ulong ByteOffset;

    /// <summary>The number of elements to be copied.</summary>
    internal uint ElementCount;

    internal GpuMapType MapType;

    internal uint DataStartIndex;

    /// <summary>The data to be set.</summary>
    internal T[] Data;

    public override void ClearForPool()
    {
        ByteOffset = 0;
        ElementCount = 0;
        MapType = GpuMapType.Read;
        DataStartIndex = 0;
        Data = null;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        if (Resource.Flags.Has(GpuResourceFlags.CpuWrite))
        {
            using (GpuStream stream = cmd.MapResource(Resource, 0, ByteOffset, MapType))
                stream.WriteRange(Data, DataStartIndex, ElementCount);
        }
        else
        {
            GpuBuffer staging = cmd.Device.Frame.StagingBuffer;
            using (GpuStream stream = cmd.MapResource(staging, 0, ByteOffset, GpuMapType.Write))
                stream.WriteRange(Data, DataStartIndex, ElementCount);

            cmd.CopyResource(staging, Resource);
        }

        return true;
    }
}
