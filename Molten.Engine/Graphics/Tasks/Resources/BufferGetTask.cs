namespace Molten.Graphics;

internal class BufferGetTask<T> : GpuResourceTask<GpuBuffer> 
    where T : unmanaged
{
    /// <summary>The number of bytes to offset the change, from the start of the provided <see cref="SrcSegment"/>.</summary>
    internal ulong ByteOffset;

    /// <summary>The number of elements to be copied.</summary>
    internal uint Count;

    /// <summary>The first index at which to start placing the retrieved data within <see cref="DestArray"/>.</summary>
    internal uint DestIndex;

    /// <summary>The destination array to store the retrieved data.</summary>
    internal T[] DestArray;

    /// <summary>
    /// Invoked when data retrieval has been completed by the assigned <see cref="GpuCommandQueue"/>.
    /// </summary>
    public event Action<T[]> OnGetData;

    public override void ClearForPool()
    {
        ByteOffset = 0;
        Count = 0;
        DestIndex = 0;
    }

    public override bool Validate()
    {
        // TODO validate if destination array is large enough
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        DestArray ??= new T[Count];
        Resource.GetDataImmediate(cmd, DestArray, DestIndex, Count, ByteOffset);

        OnGetData?.Invoke(DestArray);

        return true;
    }
}
