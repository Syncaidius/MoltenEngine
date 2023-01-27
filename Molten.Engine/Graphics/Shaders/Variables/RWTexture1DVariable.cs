namespace Molten.Graphics
{
    public class RWTexture1DVariable : RWVariable
    {
        ITexture _texture;

        internal RWTexture1DVariable(HlslShader shader) : base(shader) { }

        protected override IShaderResource OnSetUnorderedResource(object value)
        {
            _texture = value as ITexture;

            if (_texture != null)
            {
                if ((_texture.Flags & TextureFlags.AllowUAV) != TextureFlags.AllowUAV)
                    throw new InvalidOperationException("A texture cannot be passed to a RWTexture2D resource constant without .AllowUAV flags.");
            }

            return _texture;
        }
    }
}
