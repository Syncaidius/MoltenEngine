namespace Molten.Graphics
{
    public abstract class RWVariable : ShaderResourceVariable
    {
        protected RWVariable(HlslShader shader) : base(shader) { }

        protected sealed override IShaderResource OnSetResource(object value)
        {
            return OnSetUnorderedResource(value);
        }

        protected abstract IShaderResource OnSetUnorderedResource(object value);
    }
}
