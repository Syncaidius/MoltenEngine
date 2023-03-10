namespace Molten.Graphics
{
    public class RWTexture1DVariable : RWVariable
    {
        internal RWTexture1DVariable(HlslShader shader) : base(shader) { }

        protected override IShaderResource OnSetUnorderedResource(object value)
        {
            ITexture tex = value as ITexture;

            if (tex != null)
            {
                if ((tex.Flags & TextureFlags.AllowUAV) != TextureFlags.AllowUAV)
                    throw new InvalidOperationException("A texture cannot be passed to a RWTexture2D resource constant without .AllowUAV flags.");
            }

            return tex;
        }
    }
}
