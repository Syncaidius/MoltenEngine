using Molten.Graphics.Textures;

namespace Molten.Graphics
{
    public struct TextureGetTask : IGraphicsResourceTask
    {
        public GraphicsResource Staging;

        public Action<TextureData> CompleteCallback;

        public unsafe bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            ITexture texture = resource as ITexture;
            ITexture texStaging = Staging as ITexture;

            bool isStaging = texture.Flags.Has(GraphicsResourceFlags.AllReadWrite);
            bool stagingValid = Staging.Flags.Has(GraphicsResourceFlags.AllReadWrite);

            if (Staging != null)
            {
                if (!stagingValid)
                    throw new GraphicsResourceException(Staging, "Provided staging texture does not have the staging flag set.");

                // Validate dimensions.
                if (texStaging.Width != texture.Width ||
                    texStaging.Height != texture.Height ||
                    texStaging.Depth != texture.Depth)
                    throw new ResourceCopyException(resource, Staging, "Staging texture dimensions do not match current texture.");

                cmd.CopyResource(resource, Staging);
            }
            else
            {
                if (!isStaging)
                    throw new ResourceCopyException(resource, null, "A null staging texture was provided, but this is only valid if the target texture is a staging texture. A staging texture is required to retrieve data from non-staged textures.");
            }

            TextureData data = new TextureData(texture.Width, texture.Height, texture.MipMapCount, texture.ArraySize)
            {
                Flags = texture.Flags,
                Format = texture.DataFormat,
                HighestMipMap = 0,
                IsCompressed = texture.IsBlockCompressed,
            };

            uint blockSize = BCHelper.GetBlockSize(texture.DataFormat);
            uint expectedRowPitch = 4 * texture.Width; // 4-bytes per pixel * Width.
            uint expectedSlicePitch = expectedRowPitch * texture.Height;

            // Iterate over each array slice.
            for (uint a = 0; a < texture.ArraySize; a++)
            {
                // Iterate over all mip-map levels of the array slice.
                for (uint i = 0; i < texture.MipMapCount; i++)
                {
                    uint subID = (a * texture.MipMapCount) + i;
                    data.Levels[subID] = TextureSlice.FromTextureSlice(cmd, texture, texStaging, i, a);
                }
            }

            CompleteCallback?.Invoke(data);
            return false;
        }
    }
}
