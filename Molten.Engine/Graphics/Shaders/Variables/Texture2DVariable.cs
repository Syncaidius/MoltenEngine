namespace Molten.Graphics
{
    public class Texture2DVariable : ShaderResourceVariable
    {
        ITexture2D _texture;

        internal Texture2DVariable(HlslShader shader) : base(shader) { }

        protected override IShaderResource OnSetResource(object value)
        {
            if (value != null)
            {
                _texture = value as ITexture2D;

                if (_texture != null)
                {
                    if (_texture is ISwapChainSurface)
                        throw new InvalidOperationException("Texture must not be a swap chain render target.");
                }
            }
            else
            {
                _texture = null;
            }

            return _texture;
        }
    }
}
