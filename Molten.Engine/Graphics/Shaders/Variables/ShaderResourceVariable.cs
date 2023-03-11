namespace Molten.Graphics
{
    public abstract class ShaderResourceVariable : IShaderValue
    {
        IShaderResource _resource;

        public ShaderResourceVariable(HlslShader shader)
        {
            Parent = shader;
        }

        protected abstract IShaderResource OnSetResource(object value);

        /// <summary>Gets the resource bound to the variable.</summary>
        public IShaderResource Resource => _resource;

        public string Name { get; set; }

        public HlslShader Parent { get; }

        public object Value
        {
            get => _resource;
            set => _resource = OnSetResource(value);
        }
    }
}
