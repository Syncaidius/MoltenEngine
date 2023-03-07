namespace Molten.Graphics
{
    public class HlslShader : HlslGraphicsObject
    {
        public IConstantBuffer[] ConstBuffers = new IConstantBuffer[0];
        public RWVariable[] UAVs = new RWVariable[0];
        public ShaderResourceVariable[] Resources = new ShaderResourceVariable[0];
        public ShaderSamplerVariable[] SamplerVariables = new ShaderSamplerVariable[0];
        public Dictionary<string, IShaderValue> Variables = new Dictionary<string, IShaderValue>();
        
        public IShaderResource[] DefaultResources;
        HlslPass[] _passes = new HlslPass[0];

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

        internal HlslShader(GraphicsDevice device, string filename = null) : 
            base(device, GraphicsBindTypeFlags.Input)
        {
            Filename = filename ?? "";
        }

        public void AddPass(HlslPass pass)
        {
            int id = _passes?.Length ?? 0;
            Array.Resize(ref _passes, id + 1);
            _passes[id] = pass;
        }

        public override void GraphicsRelease()
        {
            for (int i = 0; i < _passes.Length; i++)
                _passes[i].Dispose();

            base.OnDispose();
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

        public HlslPass[] Passes => _passes;

        public ObjectMaterialProperties Object { get; set; }

        public LightMaterialProperties Light { get; set; }

        public SceneMaterialProperties Scene { get; set; }

        public GBufferTextureProperties Textures { get; set; }

        public SpriteBatchMaterialProperties SpriteBatch { get; set; }
    }
}
