namespace Molten.Graphics;

internal class TextureGetSliceTask : GraphicsResourceTask<GraphicsTexture>
{
    public Action<TextureSlice> OnGetData;

    public uint MipMapLevel;

    public uint ArrayIndex;

    public GraphicsMapType MapType;

    public override void ClearForPool()
    {
        OnGetData = null;
        MipMapLevel = 0;
        ArrayIndex = 0;
        MapType = GraphicsMapType.Read;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GraphicsQueue queue)
    {
        bool isStaging = Resource.Flags.Has(GraphicsResourceFlags.AllReadWrite);
        TextureSlice slice = TextureSlice.FromTextureSlice(queue, Resource, MipMapLevel, ArrayIndex, MapType);

        // Return resulting data
        OnGetData?.Invoke(slice);
        return true;
    }
}
