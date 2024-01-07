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

    /// <summary>A callback to send the retrieved data to.</summary>
    internal Action<T[]> CompletionCallback;

    /// <summary>The destination array to store the retrieved data.</summary>
    internal T[] DestArray;

    public override void ClearForPool()
    {
        ByteOffset = 0;
        Count = 0;
        DestIndex = 0;
        MapType = GraphicsMapType.Read;
        CompletionCallback = null;
    }

    public override void Validate()
    {
        throw new NotImplementedException();
    }

    protected override bool OnProcess(GraphicsQueue queue)
    {
        DestArray ??= new T[Count];

        // Now set the structured variable's data
        using (GraphicsStream stream = queue.MapResource(Resource, 0, ByteOffset, MapType))
            stream.ReadRange(DestArray, DestIndex, Count);

        CompletionCallback?.Invoke(DestArray);
        return false;
    }
}
