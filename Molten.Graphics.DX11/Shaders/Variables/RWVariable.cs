namespace Molten.Graphics
{
    internal abstract class RWVariable : ShaderResourceVariable
    {
        ContextBindableResource _uResource;

        internal RWVariable(HlslShader shader) : base(shader) { }

        protected sealed override ContextBindableResource OnSetResource(object value)
        {
            _uResource = OnSetUnorderedResource(value);

            return _uResource;
        }

        protected abstract ContextBindableResource OnSetUnorderedResource(object value);

        /// <summary>Gets the unordered access version of the resource stored in the variable (e.g. a RWBuffer or RWTexture).</summary>
        public ContextBindableResource UnorderedResource { get { return _uResource; } }
    }
}
