namespace Molten.Graphics;

internal class TextureGetSliceTask : GpuResourceTask<GpuTexture>
{
    public Action<TextureSlice> OnGetData;

    public uint MipMapLevel;

    public uint ArrayIndex;

    public GpuMapType MapType;

    public override void ClearForPool()
    {
        OnGetData = null;
        MipMapLevel = 0;
        ArrayIndex = 0;
        MapType = GpuMapType.Read;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        TextureSlice slice = TextureSlice.FromTextureSlice(cmd, Resource, MipMapLevel, ArrayIndex, MapType);

        // Return resulting data
        OnGetData?.Invoke(slice);
        return true;
    }
}
