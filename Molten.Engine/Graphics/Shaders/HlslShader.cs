namespace Molten.Graphics
{
    public abstract class HlslShader : HlslFoundation
    {
        public IConstantBuffer[] ConstBuffers = new IConstantBuffer[0];
        public ShaderResourceVariable[] Resources = new ShaderResourceVariable[0];
        public ShaderSamplerVariable[] SamplerVariables = new ShaderSamplerVariable[0];
        public Dictionary<string, IShaderValue> Variables = new Dictionary<string, IShaderValue>();
        
        public IShaderResource[] DefaultResources;

        public ShaderIOStructure InputStructure;

        public string Description { get; set; }

        public string Author { get; set; }

        public string Filename { get; }

        protected HlslShader(GraphicsDevice device, string filename = null) : 
            base(device)
        {
            Filename = filename ?? "";
        }

        public override string ToString()
        {
            return $"{GetType().Name} shader -- {Name}";
        }

        public void SetDefaultResource(IShaderResource resource, uint slot)
        {
            if (slot >= DefaultResources.Length)
                throw new IndexOutOfRangeException($"The highest slot number must be less-or-equal to the highest slot number used in the shader source code ({DefaultResources.Length}).");

            EngineUtil.ArrayResize(ref DefaultResources, slot + 1);
            DefaultResources[slot] = resource;
        }

        public IShaderResource GetDefaultResource(uint slot)
        {
            if (slot >= DefaultResources.Length)
                throw new IndexOutOfRangeException($"The highest slot number must be less-or-equal to the highest slot number used in the shader source code ({DefaultResources.Length}).");
            else
                return DefaultResources[slot];
        }

        /// <summary>Gets or sets the value of a material parameter.</summary>
        /// <value>
        /// The <see cref="IShaderValue"/>.
        /// </value>
        /// <param name="varName">The varialbe name.</param>
        /// <returns></returns>
        public IShaderValue this[string varName]
        {
            get
            {
                if (Variables.TryGetValue(varName, out IShaderValue varInstance))
                    return varInstance;
                else
                    return null;
            }

            set
            {
                if (Variables.TryGetValue(varName, out IShaderValue varInstance))
                    varInstance.Value = value;
            }
        }
    }
}
