namespace Molten.Graphics
{
    internal struct TextureGetSliceTask : IGraphicsResourceTask
    {
        public TextureDX11 StagingTexture;

        public Action<TextureSlice> CompleteCallback;

        public uint MipMapLevel;

        public uint ArrayIndex;

        public bool Process(GraphicsCommandQueue cmd, GraphicsResource resource)
        {
            TextureDX11 texture = resource as TextureDX11;

            if (!StagingTexture.HasFlags(TextureFlags.Staging) && !texture.HasFlags(TextureFlags.Staging))
                throw new TextureFlagException(StagingTexture.AccessFlags, "Provided staging texture does not have the staging flag set.");

            // If the source texture is a staging texture itself, we don't need to use the provided staging texture.
            if (!texture.HasFlags(TextureFlags.Staging))
            {
                // Validate dimensions.
                if (StagingTexture.Width != texture.Width ||
                    StagingTexture.Height != texture.Height ||
                    StagingTexture.Depth != texture.Depth)
                    throw new TextureCopyException(texture, StagingTexture, "Staging texture dimensions do not match current texture.");

                if (MipMapLevel >= texture.MipMapCount)
                    throw new TextureCopyException(texture, StagingTexture, "mip-map level must be less than the total mip-map levels of the texture.");

                if (ArrayIndex >= texture.ArraySize)
                    throw new TextureCopyException(texture, StagingTexture, "array slice must be less than the array size of the texture.");

                StagingTexture.Apply(cmd);
            }
            else
            {
                StagingTexture = null;
            }

            TextureSlice slice = texture.OnGetSliceData(cmd, StagingTexture, MipMapLevel, ArrayIndex);

            // Return resulting data
            CompleteCallback?.Invoke(slice);
            return false;
        }
    }
}
