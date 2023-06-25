using Molten.Graphics.Textures;

namespace Molten.Graphics
{
    public struct TextureGetTask : IGraphicsResourceTask
    {
        public GraphicsResource Staging;

        public Action<TextureData> CompleteCallback;

        public GraphicsMapType MapType;

        public unsafe bool Process(GraphicsQueue cmd, GraphicsResource resource)
        {
            GraphicsTexture tex = resource as GraphicsTexture;
            GraphicsTexture texStaging = Staging as GraphicsTexture;

            bool isStaging = tex.Flags.Has(GraphicsResourceFlags.AllReadWrite);
            bool stagingValid = Staging.Flags.Has(GraphicsResourceFlags.AllReadWrite);

            if (Staging != null)
            {
                if (!stagingValid)
                    throw new GraphicsResourceException(Staging, "Provided staging texture does not have the staging flag set.");

                // Validate dimensions.
                if (texStaging.Width != tex.Width ||
                    texStaging.Height != tex.Height ||
                    texStaging.Depth != tex.Depth)
                    throw new ResourceCopyException(resource, Staging, "Staging texture dimensions do not match current texture.");

                cmd.CopyResource(resource, Staging);
            }
            else
            {
                if (!isStaging)
                    throw new ResourceCopyException(resource, null, "A null staging texture was provided, but this is only valid if the target texture is a staging texture. A staging texture is required to retrieve data from non-staged textures.");
            }

            TextureData data = new TextureData(tex.Width, tex.Height, tex.Depth, tex.MipMapCount, tex.ArraySize)
            {
                Flags = tex.Flags,
                Format = tex.ResourceFormat,
                HighestMipMap = 0,
                IsCompressed = tex.IsBlockCompressed,
            };

            uint blockSize = BCHelper.GetBlockSize(tex.ResourceFormat);
            uint expectedRowPitch = 4 * tex.Width; // 4-bytes per pixel * Width.
            uint expectedSlicePitch = expectedRowPitch * tex.Height;

            // Iterate over each array slice.
            for (uint a = 0; a < tex.ArraySize; a++)
            {
                // Iterate over all mip-map levels of the array slice.
                for (uint i = 0; i < tex.MipMapCount; i++)
                {
                    uint subID = (a * tex.MipMapCount) + i;
                    data.Levels[subID] = TextureSlice.FromTextureSlice(cmd, tex, texStaging, i, a, MapType);
                }
            }

            CompleteCallback?.Invoke(data);
            return false;
        }
    }
}
