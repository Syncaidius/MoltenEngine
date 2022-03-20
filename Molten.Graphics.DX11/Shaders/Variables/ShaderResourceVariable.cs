namespace Molten.Graphics
{
    internal abstract class ShaderResourceVariable : IShaderValue
    {
        ContextBindableResource _resource;

        internal ShaderResourceVariable(HlslShader shader)
        {
            Parent = shader;
        }

        protected abstract ContextBindableResource OnSetResource(object value);

        /// <summary>Gets the resource bound to the variable.</summary>
        internal ContextBindableResource Resource { get { return _resource; } }

        public string Name { get; set; }

        public IShader Parent { get; private set; }

        public object Value
        {
            get { return Resource; }
            set
            {
                _resource = OnSetResource(value);
            }
        }
    }
}
