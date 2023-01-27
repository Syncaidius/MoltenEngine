namespace Molten.Graphics
{
    public class Texture1DVariable : ShaderResourceVariable
    {
        ITexture _texture;

        internal Texture1DVariable(HlslShader material) : base(material) { }

        protected override IShaderResource OnSetResource(object value)
        {
            _texture = value as ITexture;
            return _texture;
        }
    }
}
