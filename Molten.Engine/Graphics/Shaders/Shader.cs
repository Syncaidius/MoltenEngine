namespace Molten.Graphics;

public class Shader : GraphicsObject
{
    public RWVariable[] UAVs = [];
    public List<ShaderSampler> SharedSamplers = [];
    public Dictionary<string, ShaderVariable> Variables = new();
    public Dictionary<ShaderBindPoint, ShaderVariable> BindPointVariables = new();
    
    internal GraphicsResource[] DefaultResources;

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

    internal T CreateResourceVariable<T>(string name, uint bindPoint, uint bindSpace, ShaderBindPointType type)
        where T : ShaderResourceVariable, new()
    {
        ShaderBindPoint bp = new ShaderBindPoint(bindPoint, bindSpace, type);

        if (!BindPointVariables.TryGetValue(bp, out ShaderVariable variable))
        {
            variable = new T();
            variable.Name = name;
            variable.Parent = this;

            Variables.Add(name, variable);
            BindPointVariables.Add(bp, variable);
        }

        return variable as T;
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
            Variables.TryGetValue(varName, out ShaderVariable varInstance);
            return varInstance;
        }

        set
        {
            if (Variables.TryGetValue(varName, out ShaderVariable varInstance))
                varInstance.Value = value;
        }
    }

    public ShaderPass[] Passes => _passes;

    public ObjectMaterialProperties Object { get; set; }

    public LightMaterialProperties Light { get; set; }

    public SceneMaterialProperties Scene { get; set; }

    public GBufferTextureProperties Textures { get; set; }

    public SpriteBatchMaterialProperties SpriteBatch { get; set; }
}
