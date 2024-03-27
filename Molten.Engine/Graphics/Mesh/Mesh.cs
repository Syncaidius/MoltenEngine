using Newtonsoft.Json.Linq;

namespace Molten.Graphics;

/// <summary>A base interface for mesh implementations.</summary>
public abstract class Mesh : Renderable
{
    GpuBuffer _iBuffer;
    Shader _shader;
    ShaderBind<GpuResource>[][] _resources;
    bool _applied;

    /// <summary>
    /// Creates a new instance of <see cref="Mesh"/>, but can only be called by derived mesh classes.
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="maxVertices"></param>
    /// <param name="flags"></param>
    /// <param name="maxIndices">The maximum number of indices to allow in the current <see cref="Mesh"/>.</param>
    /// <param name="initialIndices"></param>
    protected Mesh(RenderService renderer, GpuResourceFlags flags, ushort maxVertices, uint maxIndices, ushort[] initialIndices = null) :
        base(renderer)
    {
        _resources = new ShaderBind<GpuResource>[Shader.BindTypes.Length][];
        for(int i = 0; i < _resources.Length; i++)
            _resources[i] = [];

        IndexFormat = maxIndices > 0 ? GpuIndexFormat.UInt16 : GpuIndexFormat.None;
        MaxVertices = maxVertices;
        IsDiscard = flags.IsDiscard();

        if (IndexFormat != GpuIndexFormat.None)
        {
            _iBuffer = Renderer.Device.Resources.CreateIndexBuffer(flags, maxIndices, initialIndices);

            if (initialIndices != null)
                IndexCount = (uint)initialIndices.Length;
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="Mesh"/>, but can only be called by derived mesh classes.
    /// </summary>
    /// <param name="renderer"></param>
    /// <param name="maxVertices"></param>
    /// <param name="mode"></param>
    /// <param name="maxIndices">The maximum number of indices to allow in the current <see cref="Mesh"/>.</param>
    /// <param name="initialIndices"></param>
    protected Mesh(RenderService renderer, GpuResourceFlags mode, uint maxVertices, uint maxIndices, uint[] initialIndices = null) :
        base(renderer)
    {
        _resources = new ShaderBind<GpuResource>[Shader.BindTypes.Length][];
        for (int i = 0; i < _resources.Length; i++)
            _resources[i] = [];

        IndexFormat = maxIndices > 0 ? GpuIndexFormat.UInt32 : GpuIndexFormat.None;
        MaxVertices = maxVertices;
        MaxIndices = maxIndices;

        if (IndexFormat != GpuIndexFormat.None)
        {
            _iBuffer = Renderer.Device.Resources.CreateIndexBuffer(mode, maxIndices, initialIndices);

            if (initialIndices != null)
                IndexCount = (uint)initialIndices.Length;
        }
    }

    protected void ApplyResources()
    {
        for (int i = 0; i < _shader.Bindings.Resources.Length; i++)
        {
            ref ShaderBind<ShaderResourceVariable>[] variables = ref _shader.Bindings.Resources[i];
            ref ShaderBind<GpuResource>[] resources = ref _resources[i];

            for(int r = 0; r < variables.Length; r++)
                variables[r].Object.Value = resources[r].Object;
        }
    }


    public void SetIndices<I>(I[] data) where I : unmanaged
    {
        SetIndices(data, 0, (uint)data.Length);
    }

    public void SetIndices<I>(I[] data, uint count) where I : unmanaged
    {
        SetIndices(data, 0, count);
    }

    public void SetIndices<I>(I[] data, uint startIndex, uint count) where I : unmanaged
    {
        if (_iBuffer == null)
            throw new InvalidOperationException($"Mesh is not indexed. Must be created with index format that isn't IndexBufferFormat.None.");

        IndexCount = count;
        _iBuffer.SetData(GpuPriority.StartOfFrame, data, startIndex, count, IsDiscard, 0);
    }

    protected virtual void OnApply(GpuCommandList cmd)
    {
        cmd.State.IndexBuffer.Value = _iBuffer;
    }

    protected virtual void OnPostDraw(GpuCommandList cmd)
    {
        cmd.State.IndexBuffer.Value = null;
    }

    protected virtual void OnDraw(GpuCommandList cmd)
    {
        if(_iBuffer != null)
            cmd.DrawIndexed(Shader, IndexCount);
        else
            cmd.Draw(Shader, VertexCount);
    }

    protected override sealed void OnRender(GpuCommandList cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data)
    {
        if (Shader == null)
            return;

        OnApply(cmd);
        ApplyResources();
        Shader.Object.Wvp.Value = data.RenderTransform * camera.ViewProjection;
        Shader.Object.World.Value = data.RenderTransform;
        OnDraw(cmd);
        OnPostDraw(cmd);
    }

    public virtual void Dispose()
    {
        IsVisible = false;
        _iBuffer.Dispose();
    }

    /// <summary>
    /// Gets whether the mesh is dynamic. 
    /// <para>Dynamic meshes are more performant at handling frequent data changes/updates.</para> 
    /// <para>Static meshes are preferred when their data will not change too often.</para>
    /// </summary>
    public bool IsDynamic { get; }

    /// <summary>Gets the maximum number of vertices the mesh can contain.</summary>
    public uint MaxVertices { get; }

    /// <summary>Gets the number of vertices stored in the mesh.</summary>
    public uint VertexCount { get; set; }

    public uint MaxIndices { get; set; }

    public uint IndexCount { get; set; }

    public GpuIndexFormat IndexFormat { get; }

    /// <summary>
    /// Gets or sets the material that should be used when rendering the current <see cref="Mesh"/>.
    /// </summary>
    public Shader Shader
    {
        get => _shader;
        set
        {
            if(_shader != value)
            {
                // Apply the new shader and update the resource array.
                if(value != null)
                {
                    // Ensure the bind point lists are large enough to hold the new shader's resources.
                    for(int i = 0; i < value.Bindings.Resources.Length; i++)
                    {
                        ref ShaderBind<ShaderResourceVariable>[] variables = ref value.Bindings.Resources[i];
                        ref ShaderBind<GpuResource>[] resources = ref _resources[i];

                        if(resources.Length < variables.Length)
                            Array.Resize(ref resources, variables.Length);
                    }
                }

                _shader = value;
            }
        }
    }

    public float EmissivePower { get; set; } = 1.0f;

    protected bool IsDiscard { get; }

    /// <summary>
    /// Gets or sets the resource at the specified bind point and/or bind-space
    /// </summary>
    /// <param name="bindSlot"></param>
    /// <param name="bindSpace"></param>
    /// <param name="type">The bind type.</param>
    /// <returns></returns>
    public IGpuResource this[ShaderBindType type, uint bindSlot, uint bindSpace = 0]
    {
        get
        {
            ShaderBindInfo bp = new(bindSlot, bindSpace);
            ref readonly ShaderBind<GpuResource>[] points = ref _resources[(int)type];

            for(int i = 0; i < points.Length; i++)
            {
                if(bp == points[i])
                    return points[i].Object;
            }

            return null;
        }

        set
        {
            ShaderBindInfo bp = new(bindSlot, bindSpace);
            ref ShaderBind<GpuResource>[] points = ref _resources[(int)type];

            for (int i = 0; i < points.Length; i++)
            {
                if (bp == points[i])
                {
                    points[i].Object = value as GpuResource;
                    return;
                }
            }
        }
    }
}

public class Mesh<T> : Mesh
    where T : unmanaged, IVertexType
{
    GpuBuffer _vb;

    internal Mesh(RenderService renderer, 
        GpuResourceFlags mode, ushort maxVertices, uint maxIndices,
        T[] initialVertices = null, ushort[] initialIndices = null) :
        base(renderer, mode, maxVertices, maxIndices, initialIndices)
    {
        _vb = renderer.Device.Resources.CreateVertexBuffer(mode, maxVertices, initialVertices);

        if (initialVertices != null)
            VertexCount = (uint)initialVertices.Length;
    }

    internal Mesh(RenderService renderer,
         GpuResourceFlags mode, uint maxVertices, uint maxIndices,
         T[] initialVertices = null, uint[] initialIndices = null) :
         base(renderer, mode, maxVertices, maxIndices, initialIndices)
    {
        _vb = renderer.Device.Resources.CreateVertexBuffer(mode, maxVertices, initialVertices); 
        
        if (initialVertices != null)
            VertexCount = (uint)initialVertices.Length;
    }

    public void SetVertices(T[] data)
    {
        SetVertices(data, 0, (uint)data.Length);
    }

    public void SetVertices(T[] data, uint count)
    {
        SetVertices(data, 0, count);
    }

    public void SetVertices(T[] data, uint startIndex, uint count)
    {
        VertexCount = count;
        _vb.SetData(GpuPriority.StartOfFrame, data, startIndex, count, IsDiscard, 0);
    }

    protected override void OnApply(GpuCommandList cmd)
    {
        base.OnApply(cmd);
        cmd.State.VertexBuffers[0] = _vb;
    }

    protected override void OnPostDraw(GpuCommandList cmd)
    {
        base.OnPostDraw(cmd);
        cmd.State.VertexBuffers[0] = null;
    }

    public override void Dispose()
    {
        base.Dispose();
        _vb.Dispose();
    }
}
