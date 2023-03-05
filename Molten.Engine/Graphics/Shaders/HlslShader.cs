namespace Molten.Graphics
{
    public abstract class HlslShader : HlslGraphicsObject
    {
        public IConstantBuffer[] ConstBuffers = new IConstantBuffer[0];
        public ShaderResourceVariable[] Resources = new ShaderResourceVariable[0];
        public ShaderSamplerVariable[] SamplerVariables = new ShaderSamplerVariable[0];
        public Dictionary<string, IShaderValue> Variables = new Dictionary<string, IShaderValue>();
        
        public IShaderResource[] DefaultResources;

        /// <summary>
        /// Gets a description of the shader.
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Gets the author of the shader.
        /// </summary>
        public string Author { get; internal set; }

        /// <summary>
        /// Gets the original source filename of the shader, if any.
        /// </summary>
        public string Filename { get; }

        protected HlslShader(GraphicsDevice device, string filename = null) : 
            base(device, GraphicsBindTypeFlags.Input)
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

    public abstract class HlslShader<T> : HlslShader
        where T : HlslPass
    {
        T[] _passes = new T[0];

        protected HlslShader(GraphicsDevice device, string filename = null) :
            base(device, filename)
        { }

        public void AddPass(T pass)
        {
            int id = 0;
            if (_passes == null)
            {
                _passes = new T[1];
            }
            else
            {
                id = _passes.Length;
                Array.Resize(ref _passes, _passes.Length + 1);
            }

            _passes[id] = pass;
        }

        public override void GraphicsRelease()
        {
            for (int i = 0; i < _passes.Length; i++)
                _passes[i].Dispose();

            base.OnDispose();
        }

        public T[] Passes => _passes;
    }
}
