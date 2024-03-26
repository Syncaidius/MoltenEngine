namespace Molten.Graphics;

internal class BufferSetTask<T> : GpuResourceTask<GpuBuffer>
    where T : unmanaged
{
    /// <summary>The number of bytes to offset the change, from the start of the provided <see cref="Segment"/>.</summary>
    internal ulong ByteOffset;

    /// <summary>The number of elements to be copied.</summary>
    internal ulong ElementCount;

    internal bool Discard;

    internal uint DataStartIndex;

    /// <summary>The data to be set.</summary>
    internal T[] Data;

    public override void ClearForPool()
    {
        ByteOffset = 0;
        ElementCount = 0;
        Discard = false;
        DataStartIndex = 0;
        Data = null;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        Resource.SetDataImmediate(cmd, Data, DataStartIndex, ElementCount, Discard, ByteOffset);

        return true;
    }
}
