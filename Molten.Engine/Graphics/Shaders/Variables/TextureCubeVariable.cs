namespace Molten.Graphics
{
    public class TextureCubeVariable : ShaderResourceVariable
    {
        ITextureCube _texture;

        internal TextureCubeVariable(HlslShader shader) : base(shader) { }

        protected override IShaderResource OnSetResource(object value)
        {
            if (value != null)
                _texture = value as ITextureCube;
            else
                _texture = null;

            return _texture;
        }
    }
}
