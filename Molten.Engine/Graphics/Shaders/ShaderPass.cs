using Molten.Graphics.Shaders;
using System.Collections;

namespace Molten.Graphics;

/// <summary>
/// An a base class implementation for key shader components, such as materials, material passes or compute tasks.
/// </summary>
public abstract class ShaderPass : GraphicsObject, IEnumerable<ShaderComposition>, IEnumerable<ShaderType>
{
    /// <summary>
    /// A callback that is used by <see cref="ShaderPass"/> when it has finished its draw/dispatch call.
    /// </summary>
    /// <param name="pass">The pass that was completed.</param>
    /// <param name="customInfo">Custom information that can be passed between shader passes.</param>
    public delegate void OnCompletedCallback(ShaderPass pass, GraphicsQueue.CustomDrawInfo customInfo);

    /// <summary>
    /// The texture samplers to be used with the shader/component.
    /// </summary>
    public ShaderSampler[] Samplers;
    Dictionary<ShaderType, ShaderComposition> _compositions;
    public unsafe void* InputByteCode;

    ShaderFormatLayout _formatLayout;

    /// <summary>
    /// Invoked when the current <see cref="ShaderPass"/> has finished its draw/dispatch call.
    /// </summary>
    public event OnCompletedCallback OnCompleted;

    /// <summary>
    /// Creates a new instance of <see cref="ShaderPass"/>. Can only be called by a derived class.
    /// </summary>
    /// <param name="parent">The parnet shader that owns this new instance of <see cref="ShaderPass"/>.</param>
    /// <param name="name">The readable name to give to the <see cref="ShaderPass"/>.</param>
    protected ShaderPass(Shader parent, string name) : 
        base(parent.Device)
    {
        Samplers = new ShaderSampler[0];
        Parent = parent;
        Name = name;
        IsEnabled = true;
        _compositions = new Dictionary<ShaderType, ShaderComposition>();
    }

    internal unsafe void Initialize(ref ShaderPassParameters parameters)
    {
        ComputeGroups = new Vector3UI(parameters.GroupsX, parameters.GroupsY, parameters.GroupsZ);
        Topology = parameters.Topology;
        RasterizedStreamOutput = parameters.RasterizedStreamOutput;

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

    public bool HasComposition(ShaderType type)
    {
        return _compositions.ContainsKey(type);
    }

    /// <summary>
    /// Gets a <see cref="ShaderComposition"/> from the current <see cref="ShaderPass"/>. 
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
    /// Gets the number of compositions in the current <see cref="ShaderPass"/>. 
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
    /// Gets the parent <see cref="Shader"/> that the current <see cref="ShaderPass"/> belongs to.
    /// </summary>
    public Shader Parent { get; }

    /// <summary>
    /// Gets whether the current <see cref="ShaderPass"/> is a compute pass.
    /// </summary>
    public bool IsCompute { get; internal set; }

    /// <summary>
    /// Gets the compute group sizes for the current <see cref="ShaderPass"/>. This has no effect if <see cref="IsCompute"/> is false.
    /// </summary>
    public Vector3UI ComputeGroups { get; private set; }

    /// <summary>
    /// Gets the vertex <see cref="PrimitiveTopology"/> that the current <see cref="ShaderPass"/> will use when rendering mesh vertices.
    /// </summary>
    public PrimitiveTopology Topology { get; private set; }

    /// <summary>
    /// Gets the buffer/stream ID to use when outputting from a geometry shader stream to a pixel/fragment shader. 
    /// <para>This is ignored if a pixel/fragment shader is not present.</para>
    /// </summary>
    public uint RasterizedStreamOutput { get; private set; }

    /// <summary>
    /// Gets the format layout used by the pixel/fragment stage of the current <see cref="ShaderPass"/>, if present.
    /// </summary>
    public ref ShaderFormatLayout FormatLayout => ref _formatLayout;
}
