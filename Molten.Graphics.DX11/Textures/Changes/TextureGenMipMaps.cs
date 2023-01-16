namespace Molten.Graphics
{
    internal struct TexturegenMipMaps : ITextureTask
    {
        public bool Process(CommandQueueDX11 pipe, TextureBase texture)
        {
            texture.GenerateMipMaps(pipe);
            return true;
        }
    }
}
