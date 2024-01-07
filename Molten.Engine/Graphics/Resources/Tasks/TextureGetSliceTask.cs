namespace Molten.Graphics;

internal class TextureGetSliceTask : GraphicsResourceTask<GraphicsTexture>
{
    public Action<TextureSlice> CompleteCallback;

    public uint MipMapLevel;

    public uint ArrayIndex;

    public GraphicsMapType MapType;

    public override void ClearForPool()
    {
        CompleteCallback = null;
        MipMapLevel = 0;
        ArrayIndex = 0;
        MapType = GraphicsMapType.Read;
    }

    public override void Validate()
    {
        throw new NotImplementedException();
    }

    protected override bool OnProcess(GraphicsQueue queue)
    {
        bool isStaging = Resource.Flags.Has(GraphicsResourceFlags.AllReadWrite);
        TextureSlice slice = TextureSlice.FromTextureSlice(queue, Resource, MipMapLevel, ArrayIndex, MapType);

        // Return resulting data
        CompleteCallback?.Invoke(slice);
        return false;
    }
}
