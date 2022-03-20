namespace Molten.Graphics
{
    internal class Texture2DVariable : ShaderResourceVariable
    {
        Texture2D _texture;

        internal Texture2DVariable(HlslShader shader) : base(shader) { }

        protected override ContextBindableResource OnSetResource(object value)
        {
            if (value != null)
            {
                _texture = value as Texture2D;

                if (_texture != null)
                {
                    if (_texture is SwapChainSurface)
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
