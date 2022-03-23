namespace Molten.Graphics
{
    internal struct TextureGetSlice : ITextureTask
    {
        public TextureBase StagingTexture;

        public Action<TextureData.Slice> Callback;

        public uint MipMapLevel;

        public uint ArrayIndex;

        public bool Process(DeviceContext pipe, TextureBase texture)
        {
            if (!StagingTexture.HasFlags(TextureFlags.Staging) && !texture.HasFlags(TextureFlags.Staging))
                throw new TextureFlagException(StagingTexture.Flags, "Provided staging texture does not have the staging flag set.");

            if (texture.HasFlags(TextureFlags.Staging))
                StagingTexture = null;

            // Validate dimensions.
            if (StagingTexture.Width != texture.Width ||
                StagingTexture.Height != texture.Height ||
                StagingTexture.Depth != texture.Depth)
                throw new TextureCopyException(texture, StagingTexture, "Staging texture dimensions do not match current texture.");

            if (MipMapLevel >= texture.MipMapCount)
                throw new TextureCopyException(texture, StagingTexture, "mip-map level must be less than the total mip-map levels of the texture.");

            if (ArrayIndex >= texture.ArraySize)
                throw new TextureCopyException(texture, StagingTexture, "array slice must be less than the array size of the texture.");

            StagingTexture.Apply(pipe);

            TextureData.Slice slice = texture.GetSliceData(pipe, StagingTexture, MipMapLevel, ArrayIndex);

            // Return resulting data
            Callback(slice);

            return false;
        }
    }
}
