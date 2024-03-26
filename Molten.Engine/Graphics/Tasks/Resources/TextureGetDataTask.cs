using Molten.Graphics.Textures;

namespace Molten.Graphics;

internal class TextureGetDataTask : GpuResourceTask<GpuTexture>
{
    public Action<TextureData> OnGetData;

    public GpuMapType MapType;

    public override void ClearForPool()
    {
        OnGetData = null;
        MapType = GpuMapType.Read;
    }

    public override bool Validate()
    {
        return true;
    }

    protected override bool OnProcess(RenderService renderer, GpuCommandList cmd)
    {
        TextureData data = new TextureData(Resource.Width, Resource.Height, Resource.Depth, Resource.MipMapCount, Resource.ArraySize)
        {
            Flags = Resource.Flags,
            Format = Resource.ResourceFormat,
            HighestMipMap = 0,
            IsCompressed = Resource.IsBlockCompressed,
        };

        uint blockSize = BCHelper.GetBlockSize(Resource.ResourceFormat);
        uint expectedRowPitch = 4 * Resource.Width; // 4-bytes per pixel * Width.
        uint expectedSlicePitch = expectedRowPitch * Resource.Height;

        // Iterate over each array slice.
        for (uint a = 0; a < Resource.ArraySize; a++)
        {
            // Iterate over all mip-map levels of the array slice.
            for (uint i = 0; i < Resource.MipMapCount; i++)
            {
                uint subID = (a * Resource.MipMapCount) + i;
                data.Levels[subID] = TextureSlice.FromTextureSlice(cmd, Resource, i, a, MapType);
            }
        }

        // TODO Perform CPU sync here.

        OnGetData?.Invoke(data);
        return true;
    }
}
