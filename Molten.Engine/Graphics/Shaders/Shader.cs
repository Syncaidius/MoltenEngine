namespace Molten.Graphics;

public class Shader : GraphicsObject
{
    public static readonly ShaderBindType[] BindTypes = Enum.GetValues<ShaderBindType>();

    public RWVariable[] UAVs = [];
    public List<ShaderSampler> SharedSamplers = [];
    public Dictionary<string, ShaderVariable> Variables = new();

    public ShaderBindManager Bindings { get; }

    ShaderPass[] _passes = [];

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

    internal Shader(GraphicsDevice device, ShaderDefinition def, string filename = null) : 
        base(device)
    {
        Bindings = new ShaderBindManager(this, null);
        Name = def.Name;
        Description = def.Description;
        Author = def.Author;
        Filename = filename ?? "";
    }

    internal void LinkSampler(ShaderSamplerParameters parameters)
    {
        // Find an existing sampler with the same settings.
        for (int i = 0; i < SharedSamplers.Count; i++)
        {
            ShaderSampler s = SharedSamplers[i];
            if (s.Equals(parameters))
                parameters.LinkedSampler = s;
        }

        // Create a new sampler
        if (parameters.LinkedSampler == null)
        {
            parameters.LinkedSampler = Device.CreateSampler(parameters);
            SharedSamplers.Add(parameters.LinkedSampler);
        }
    }

    public void AddPass(ShaderPass pass)
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
            Variables.TryGetValue(varName, out ShaderVariable varInstance);
            return varInstance;
        }

        set
        {
            if (Variables.TryGetValue(varName, out ShaderVariable varInstance))
                varInstance.Value = value;
        }
    }

    /// <summary>
    /// Gets or sets a default resource at the specified bind point and/or bind-space. This is used when a resource is not explicitly set.
    /// </summary>
    /// <param name="bindSlot"></param>
    /// <param name="bindSpace"></param>
    /// <param name="type">The bind type.</param>
    /// <returns></returns>
    public IGraphicsResource this[ShaderBindType type, uint bindSlot, uint bindSpace = 0]
    {
        get
        {
            ShaderBindInfo bp = new(bindSlot, bindSpace);
            ref readonly ShaderBind<ShaderResourceVariable>[] points = ref Bindings.Resources[(int)type];

            for (int i = 0; i < points.Length; i++)
            {
                if (bp == points[i])
                    return points[i].Object.Resource;
            }

            return null;
        }

        set
        {
            ShaderBindInfo bp = new(bindSlot, bindSpace);
            ref ShaderBind<ShaderResourceVariable>[] points = ref Bindings.Resources[(int)type];

            for (int i = 0; i < points.Length; i++)
            {
                if (bp == points[i])
                {
                    points[i].Object.DefaultValue = value as GraphicsResource;
                    return;
                }
            }
        }
    }

    public ShaderPass[] Passes => _passes;

    public ObjectMaterialProperties Object { get; set; }

    public LightMaterialProperties Light { get; set; }

    public SceneMaterialProperties Scene { get; set; }

    public GBufferTextureProperties Textures { get; set; }

    public SpriteBatchMaterialProperties SpriteBatch { get; set; }
}
