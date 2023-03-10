namespace Molten.Graphics
{
    public class RWTexture2DVariable : RWVariable
    {
        internal RWTexture2DVariable(HlslShader shader) : base(shader) { }

        protected override IShaderResource OnSetUnorderedResource(object value)
        {
            ITexture2D tex = value as ITexture2D;

            if (tex != null)
            {
                if (tex is ISwapChainSurface)
                    throw new InvalidOperationException("Texture must not be a swap chain render target.");
                else if ((tex.Flags & TextureFlags.AllowUAV) != TextureFlags.AllowUAV)
                    throw new InvalidOperationException("A texture cannot be passed to a RWTexture2D resource constant without .AllowUAV flags.");
            }

            return tex;
        }
    }
}
