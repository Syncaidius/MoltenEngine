namespace Molten.Graphics
{
    internal struct TextureGet : ITextureTask
    {
        public TextureBase StagingTexture;

        public Action<TextureData> Callback;

        public bool Process(CommandQueueDX11 cmd, TextureBase texture)
        {
            if (!StagingTexture.HasFlags(TextureFlags.Staging))
                throw new TextureFlagException(StagingTexture.Flags, "Provided staging texture does not have the staging flag set.");

            // Validate dimensions.
            if (StagingTexture.Width != texture.Width ||
                StagingTexture.Height != texture.Height ||
                StagingTexture.Depth != texture.Depth)
                throw new TextureCopyException(texture, StagingTexture, "Staging texture dimensions do not match current texture.");

            TextureData data = texture.GetAllData(cmd, StagingTexture);
            Callback(data);

            return false;
        }
    }
}
