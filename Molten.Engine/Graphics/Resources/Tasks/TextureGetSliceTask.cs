namespace Molten.Graphics;

internal struct TextureGetSliceTask : IGraphicsResourceTask
{
    public Action<TextureSlice> CompleteCallback;

    public uint MipMapLevel;

    public uint ArrayIndex;

    public GraphicsMapType MapType;

    public bool Process(GraphicsQueue cmd, GraphicsResource resource)
    {
        GraphicsTexture texture = resource as GraphicsTexture;

        bool isStaging = texture.Flags.Has(GraphicsResourceFlags.AllReadWrite);
        TextureSlice slice = TextureSlice.FromTextureSlice(cmd, texture, MipMapLevel, ArrayIndex, MapType);

        // Return resulting data
        CompleteCallback?.Invoke(slice);
        return false;
    }
}
