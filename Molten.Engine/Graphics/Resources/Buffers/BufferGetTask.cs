namespace Molten.Graphics;

internal class BufferGetTask<T> : GraphicsResourceTask<GraphicsBuffer> 
    where T : unmanaged
{
    /// <summary>The number of bytes to offset the change, from the start of the provided <see cref="SrcSegment"/>.</summary>
    internal uint ByteOffset;
    /// <summary>The number of elements to be copied.</summary>
    internal uint Count;

    /// <summary>The first index at which to start placing the retrieved data within <see cref="DestArray"/>.</summary>
    internal uint DestIndex;

    internal GraphicsMapType MapType;

    /// <summary>The destination array to store the retrieved data.</summary>
    internal T[] DestArray;

    /// <summary>
    /// Invoked when data retrieval has been completed by the assigned <see cref="GraphicsQueue"/>.
    /// </summary>
    public event Action<T[]> OnGetData;

    public override void ClearForPool()
    {
        ByteOffset = 0;
        Count = 0;
        DestIndex = 0;
        MapType = GraphicsMapType.Read;
    }

    public override void Validate()
    {
        // TODO validate if destination array is large enough
        throw new NotImplementedException();
    }

    protected override bool OnProcess(RenderService renderer, GraphicsQueue queue)
    {
        DestArray ??= new T[Count];

        // Now set the structured variable's data
        using (GraphicsStream stream = queue.MapResource(Resource, 0, ByteOffset, MapType))
            stream.ReadRange(DestArray, DestIndex, Count);

        OnGetData?.Invoke(DestArray);

        return true;
    }
}
