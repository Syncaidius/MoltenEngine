using Molten.Graphics.Textures;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal struct TextureGetTask : IGraphicsResourceTask
    {
        public TextureDX11 Staging;

        public Action<TextureData> CompleteCallback;

        public unsafe bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            TextureDX11 texture = resource as TextureDX11;
            CommandQueueDX11 cmdDX11 = cmd as CommandQueueDX11;

            bool isStaging = texture.Flags.Has(GraphicsResourceFlags.AllReadWrite);
            bool stagingValid = Staging.Flags.Has(GraphicsResourceFlags.AllReadWrite);

            if (Staging != null)
            {
                if (!stagingValid)
                    throw new TextureFlagException(Staging.Flags, "Provided staging texture does not have the staging flag set.");

                // Validate dimensions.
                if (Staging.Width != texture.Width ||
                    Staging.Height != texture.Height ||
                    Staging.Depth != texture.Depth)
                    throw new TextureCopyException(texture, Staging, "Staging texture dimensions do not match current texture.");

                if (texture.ResourcePtr == null)
                    texture.Apply(cmd);

                Staging.Apply(cmd);
                cmdDX11.CopyResource(texture, Staging);
            }
            else
            {
                if (!isStaging)
                    throw new TextureCopyException(texture, null, "A null staging texture was provided, but this is only valid if the target texture is a staging texture. A staging texture is required to retrieve data from non-staged textures.");
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
                    data.Levels[subID] = texture.OnGetSliceData(cmd, Staging, i, a);
                }
            }

            CompleteCallback?.Invoke(data);
            return false;
        }
    }
}
