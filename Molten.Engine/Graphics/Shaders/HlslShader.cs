namespace Molten.Graphics
{
    public class HlslShader : GraphicsObject
    {
        public IConstantBuffer[] ConstBuffers = new IConstantBuffer[0];
        public RWVariable[] UAVs = new RWVariable[0];
        public ShaderResourceVariable[] Resources = new ShaderResourceVariable[0];
        public ShaderSamplerVariable[] SamplerVariables = new ShaderSamplerVariable[0];
        public Dictionary<string, ShaderVariable> Variables = new Dictionary<string, ShaderVariable>();
        
        internal GraphicsResource[] DefaultResources;

        HlslPass[] _passes = new HlslPass[0];

        /// <summary>
        /// Gets a description of the shader.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the author of the shader.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Gets the original source filename of the shader, if any.
        /// </summary>
        public string Filename { get; }

        internal HlslShader(GraphicsDevice device, ShaderDefinition def, string filename = null) : 
            base(device)
        {
            Name = def.Name;
            Description = def.Description;
            Author = def.Author;

            Filename = filename ?? "";
        }

        public void AddPass(HlslPass pass)
        {
            int id = _passes?.Length ?? 0;
            Array.Resize(ref _passes, id + 1);
            _passes[id] = pass;
        }

        protected override void OnGraphicsRelease()
        {
            for (int i = 0; i < _passes.Length; i++)
                _passes[i].Dispose();
        }

        public void SetDefaultResource(IGraphicsResource resource, uint slot)
        {
            if (slot >= DefaultResources.Length)
                throw new IndexOutOfRangeException($"The highest slot number must be less-or-equal to the highest slot number used in the shader source code ({DefaultResources.Length}).");

            EngineUtil.ArrayResize(ref DefaultResources, slot + 1);
            DefaultResources[slot] = resource as GraphicsResource;
        }

        public GraphicsResource GetDefaultResource(uint slot)
        {
            if (slot >= DefaultResources.Length)
                throw new IndexOutOfRangeException($"The highest slot number must be less-or-equal to the highest slot number used in the shader source code ({DefaultResources.Length}).");
            else
                return DefaultResources[slot];
        }

        /// <summary>Gets or sets the value of a material parameter.</summary>
        /// <value>
        /// The <see cref="ShaderVariable"/>.
        /// </value>
        /// <param name="varName">The variable name.</param>
        /// <returns></returns>
        public ShaderVariable this[string varName]
        {
            get
            {
                if (Variables.TryGetValue(varName, out ShaderVariable varInstance))
                    return varInstance;
                else
                    return null;
            }

            set
            {
                if (Variables.TryGetValue(varName, out ShaderVariable varInstance))
                    varInstance.Value = value;
            }
        }

        public HlslPass[] Passes => _passes;

        public ObjectMaterialProperties Object { get; set; }

        public LightMaterialProperties Light { get; set; }

        public SceneMaterialProperties Scene { get; set; }

        public GBufferTextureProperties Textures { get; set; }

        public SpriteBatchMaterialProperties SpriteBatch { get; set; }
    }
}
