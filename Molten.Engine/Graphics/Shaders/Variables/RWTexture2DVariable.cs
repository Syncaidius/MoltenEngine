namespace Molten.Graphics
{
    public class RWTexture2DVariable : RWVariable
    {
        ITexture2D _texture;

        internal RWTexture2DVariable(HlslShader shader) : base(shader) { }

        protected override IShaderResource OnSetUnorderedResource(object value)
        {
            _texture = value as ITexture2D;

            if (_texture != null)
            {
                if (_texture is ISwapChainSurface)
                    throw new InvalidOperationException("Texture must not be a swap chain render target.");
                else if ((_texture.Flags & TextureFlags.AllowUAV) != TextureFlags.AllowUAV)
                    throw new InvalidOperationException("A texture cannot be passed to a RWTexture2D resource constant without .AllowUAV flags.");
            }

            return _texture;
        }
    }
}
