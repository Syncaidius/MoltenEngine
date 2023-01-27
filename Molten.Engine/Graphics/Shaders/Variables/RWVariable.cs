namespace Molten.Graphics
{
    public abstract class RWVariable : ShaderResourceVariable
    {
        IShaderResource _uResource;

        protected RWVariable(HlslShader shader) : base(shader) { }

        protected sealed override IShaderResource OnSetResource(object value)
        {
            _uResource = OnSetUnorderedResource(value);
            return _uResource;
        }

        protected abstract IShaderResource OnSetUnorderedResource(object value);

        /// <summary>Gets the unordered access version of the resource stored in the variable (e.g. a RWBuffer or RWTexture).</summary>
        public IShaderResource UnorderedResource => _uResource;
    }
}
