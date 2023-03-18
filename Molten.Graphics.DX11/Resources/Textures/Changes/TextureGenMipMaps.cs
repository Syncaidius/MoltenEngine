namespace Molten.Graphics
{
    internal struct TexturegenMipMaps : ITextureTask
    {
        public bool Process(CommandQueueDX11 cmd, TextureDX11 texture)
        {
            texture.GenerateMipMaps(cmd);
            return true;
        }
    }
}
