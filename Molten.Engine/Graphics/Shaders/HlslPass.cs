using System.Collections;

namespace Molten.Graphics;

/// <summary>
/// An a base class implementation for key shader components, such as materials, material passes or compute tasks.
/// </summary>
public abstract class HlslPass : GraphicsObject, IEnumerable<ShaderComposition>, IEnumerable<ShaderType>
{
    /// <summary>
    /// A callback that is used by <see cref="HlslPass"/> when it has finished its draw/dispatch call.
    /// </summary>
    /// <param name="pass">The pass that was completed.</param>
    /// <param name="customInfo">Custom information that can be passed between shader passes.</param>
    public delegate void OnCompletedCallback(HlslPass pass, GraphicsQueue.CustomDrawInfo customInfo);

    /// <summary>
    /// The texture samplers to be used with the shader/component.
    /// </summary>
    public ShaderSampler[] Samplers;
    Dictionary<ShaderType, ShaderComposition> _compositions;
    public unsafe void* InputByteCode;

    /// <summary>
    /// Invoked when the current <see cref="HlslPass"/> has finished its draw/dispatch call.
    /// </summary>
    public event OnCompletedCallback OnCompleted;

    /// <summary>
    /// Creates a new instance of <see cref="HlslPass"/>. Can only be called by a derived class.
    /// </summary>
    /// <param name="parent">The parnet shader that owns this new instance of <see cref="HlslPass"/>.</param>
    /// <param name="name">The readable name to give to the <see cref="HlslPass"/>.</param>
    protected HlslPass(HlslShader parent, string name) : 
        base(parent.Device)
    {
        Samplers = new ShaderSampler[0];
        Parent = parent;
        Name = name;
        IsEnabled = true;
        _compositions = new Dictionary<ShaderType, ShaderComposition>();
    }

    internal void Initialize(GraphicsStatePreset preset, PrimitiveTopology topology)
    {
        Initialize(preset, topology, Vector3UI.Zero);
    }

    internal void Initialize(GraphicsStatePreset preset, PrimitiveTopology topology, Vector3UI computeGroups)
    {
        ShaderPassParameters p = new ShaderPassParameters(preset, topology);
        p.GroupsX = computeGroups.X;
        p.GroupsY = computeGroups.Y;
        p.GroupsZ = computeGroups.Z;

        Initialize(ref p);
    }

    internal void Initialize(ref ShaderPassParameters parameters)
    {
        ComputeGroups = new Vector3UI(parameters.GroupsX, parameters.GroupsY, parameters.GroupsZ);
        Topology = parameters.Topology;

        OnInitialize(ref parameters);
    }

    protected abstract void OnInitialize(ref ShaderPassParameters parameters);

    public void InvokeCompleted(GraphicsQueue.CustomDrawInfo customInfo)
    {
        OnCompleted?.Invoke(this, customInfo);
    }

    internal ShaderComposition AddComposition(ShaderType type)
    {
        if (!_compositions.TryGetValue(type, out ShaderComposition comp))
        {
            comp = new ShaderComposition(this, type);
            _compositions.Add(type, comp);
        }

        return comp;
    }

    protected override void OnGraphicsRelease()
    {
        foreach (ShaderComposition c in _compositions.Values)
            c.Dispose();
    }

    public IEnumerator<ShaderComposition> GetEnumerator()
    {
        return _compositions.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _compositions.Keys.GetEnumerator();
    }

    IEnumerator<ShaderType> IEnumerable<ShaderType>.GetEnumerator()
    {
        return _compositions.Keys.GetEnumerator();
    }

    /// <summary>
    /// Gets a <see cref="ShaderComposition"/> from the current <see cref="HlslPass"/>. 
    /// <para>Returns null if no composition exists for the specified <see cref="ShaderType"/>.</para>
    /// </summary>
    /// <param name="type">The type of shader composition to retrieve.</param>
    /// <returns></returns>
    public ShaderComposition this[ShaderType type]
    {
        get
        {
            _compositions.TryGetValue(type, out ShaderComposition comp);
            return comp;
        }
    }

    /// <summary>
    /// Gets the number of compositions in the current <see cref="HlslPass"/>. 
    /// Each composition represents a shader pipeline stage. For example, vertex, geometry or fragment/pixel stages.
    /// </summary>
    public int CompositionCount => _compositions.Count;

    /// <summary>
    /// Gets or sets the type of geometry shader primitives to output.
    /// </summary>
    public GeometryHullTopology GeometryPrimitive { get; set; }

    /// <summary>
    /// Gets or sets the depth write permission. the default value is <see cref="GraphicsDepthWritePermission.Enabled"/>.
    /// </summary>
    public GraphicsDepthWritePermission WritePermission { get; set; } = GraphicsDepthWritePermission.Enabled;

    /// <summary>
    /// Gets or sets the number of iterations the shader/component should be run.
    /// </summary>
    public int Iterations { get; set; } = 1;

    /// <summary>Gets or sets whether or not the pass will be run.</summary>
    /// <value>
    ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
    /// </value>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets the parent <see cref="HlslShader"/> that the current <see cref="HlslPass"/> belongs to.
    /// </summary>
    public HlslShader Parent { get; }

    /// <summary>
    /// Gets whether the current <see cref="HlslPass"/> is a compute pass.
    /// </summary>
    public bool IsCompute { get; internal set; }

    /// <summary>
    /// Gets the compute group sizes for the current <see cref="HlslPass"/>. This has no effect if <see cref="IsCompute"/> is false.
    /// </summary>
    public Vector3UI ComputeGroups { get; private set; }

    /// <summary>
    /// Gets the vertex <see cref="PrimitiveTopology"/> that the current <see cref="HlslPass"/> will use when rendering mesh vertices.
    /// </summary>
    public PrimitiveTopology Topology { get; private set; }
}
