namespace Molten.Graphics
{
    internal struct TexturegenMipMaps : ITextureTask
    {
        public bool Process(DeviceContext pipe, TextureBase texture)
        {
            texture.GenerateMipMaps(pipe);
            return true;
        }
    }
}
