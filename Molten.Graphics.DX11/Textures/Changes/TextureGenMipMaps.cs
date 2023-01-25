namespace Molten.Graphics
{
    internal struct TexturegenMipMaps : ITextureTask
    {
        public bool Process(CommandQueueDX11 cmd, TextureBase texture)
        {
            texture.GenerateMipMaps(cmd);
            return true;
        }
    }
}
