using Newtonsoft.Json.Linq;

namespace Molten.Graphics;

public abstract partial class GpuCommandList : GpuObject
{
    /// <summary>
    /// A container for storing application data to share between completion callbacks of <see cref="Shader"/> passes.
    /// </summary>
    public class CustomDrawInfo
    {
        /// <summary>
        /// Custom compute dispatch group sizes. 
        /// <para>
        /// Any dimension that is 0 will default to the one provided by the shader's definition, if any.</para>
        /// </summary>
        public Vector3UI ComputeGroups;

        /// <summary>
        /// Gets a dictionary of custom values.
        /// </summary>
        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        internal void Reset()
        {
            ComputeGroups = Vector3UI.Zero;
            Values.Clear();
        }
    }

    protected class BatchDrawInfo
    {
        public bool Began { get; internal set; }

        public Vector3UI ComputeGroups;

        public CustomDrawInfo Custom { get; } = new CustomDrawInfo();

        public void Reset()
        {
            Began = false;
            Custom.Reset();
        }
    }

    GpuState _state;
    Stack<GpuState> _stateStack;
    Stack<GpuState> _freeStateStack;

    protected GpuCommandList(GpuDevice device) :
        base(device)
    {
        Profiler = new GraphicsQueueProfiler(); 
        DrawInfo = new BatchDrawInfo();

        _state = new GpuState(device);
        _stateStack = new Stack<GpuState>();
        _freeStateStack = new Stack<GpuState>();
    }

    public void PushState(GpuState newest = null)
    {
        _stateStack.Push(_state);

        if (_freeStateStack.Count > 0)
        {
            GpuState clone = _freeStateStack.Pop();
            _state.CopyTo(clone);
        }
        else
        {
            _state = newest ?? _state.Clone();
        }
    }

    public void PopState()
    {
        if (_stateStack.Count == 0)
            throw new InvalidOperationException($"There are no states to pop from the current {nameof(GpuCommandList)}.");

        _freeStateStack.Push(_state);
        _state = _stateStack.Pop();
    }

    internal void ResetState()
    {
        while (_stateStack.Count > 0)
        {
            _freeStateStack.Push(_state);
            _state = _stateStack.Pop();
        }

        _state.Reset();
        OnResetState();
    }

    protected GpuBindResult ApplyState(Shader shader, QueueValidationMode mode, Action callback)
    {
        GpuBindResult vResult = GpuBindResult.Successful;

        if (!DrawInfo.Began)
            throw new GpuCommandListException(this, $"{GetType().Name}: BeginDraw() must be called before calling {nameof(Draw)}()");

        State.Shader.Value = shader;
        bool shaderChanged = State.Shader.Bind();

        if (State.Shader.BoundValue == null)
            return GpuBindResult.NoShader;

        // Re-render the same material for mat.Iterations.
        BeginEvent($"{mode} Call");
        for (uint i = 0; i < shader.Passes.Length; i++)
        {
            ShaderPass pass = shader.Passes[i];
            if (!pass.IsEnabled)
            {
                SetMarker($"Pass {i} - Skipped (Disabled)");
                continue;
            }

            if (pass.IsCompute)
            {
                BeginEvent($"Pass {i} - Compute");
                vResult = DoComputePass(pass);
                EndEvent();
            }
            else
            {
                // Skip non-compute passes when we're in compute-only mode.
                if (mode == QueueValidationMode.Compute)
                {
                    SetMarker($"Pass {i} - Skipped (Compute-Only-mode)");
                    continue;
                };

                BeginEvent($"Pass {i} - Render");
                vResult = DoRenderPass(pass, mode, callback);
                EndEvent();
            }

            if (vResult != GpuBindResult.Successful)
            {
                Device.Log.Warning($"{mode} call failed with result: {vResult} -- Iteration: Pass {i}/{shader.Passes.Length} -- " +
                $"Shader: {shader.Name} -- Topology: {pass.Topology} -- IsCompute: {pass.IsCompute}");
                break;
            }

            pass.InvokeCompleted(DrawInfo.Custom);
        }
        EndEvent();

        return vResult;
    }

    protected abstract unsafe GpuBindResult DoRenderPass(ShaderPass pass, QueueValidationMode mode, Action callback);

    protected abstract GpuBindResult DoComputePass(ShaderPass pass);

    /// <summary>
    /// Starts recording commands in the current <see cref="GpuCommandList"/>.
    /// </summary>
    /// <param name="flags">The flags to apply to the underlying command segment.</param>
    /// <exception cref="GpuCommandListException"></exception>
    public virtual void Begin()
    {
#if DEBUG
        if (DrawInfo.Began)
            throw new GpuCommandListException(this, $"{nameof(GpuCommandList)}: End() must be called before the next Begin() call.");
#endif

        DrawInfo.Began = true;
    }

    public virtual void End()
    {
#if DEBUG
        if (!DrawInfo.Began)
            throw new GpuCommandListException(this, $"{nameof(GpuCommandList)}: BeginDraw() must be called before EndDraw().");
#endif

        DrawInfo.Reset();
    }

    /// <summary>Generates mip maps for the texture via the provided <see cref="GpuTexture"/>, if allowed.</summary>
    /// <param name="texture">The target texture for mip-map generation.</param>
    protected internal abstract void OnGenerateMipmaps(GpuTexture texture);

    /// <summary>Generates mip maps for the texture via the current <see cref="GpuTexture"/>, if allowed.</summary>
    /// <param name="texture">The texture for which to generate mip-maps.</param>
    /// <param name="priority">The priority of the copy operation.</param>
    /// <param name="callback">A callback to run once the operation has completed.</param>
    public void GenerateMipMaps(GpuTexture texture, GpuPriority priority, GpuTask.EventHandler callback = null)
    {
        if (!texture.Flags.Has(GpuResourceFlags.MipMapGeneration))
            throw new Exception("Cannot generate mip-maps for texture. Must have flag: TextureFlags.AllowMipMapGeneration.");

        GenerateMipMapsTask task = Device.Tasks.Get<GenerateMipMapsTask>();
        task.OnCompleted += callback;
        Device.Tasks.Push(priority, task);
    }

    /// <summary>
    /// Appends the commands within the provided <see cref="GpuCommandList"/> into the current <see cref="GpuCommandList"/>.
    /// </summary>
    /// <param name="cmd"></param>
    public abstract void Execute(GpuCommandList cmd);

    /// <summary>Draw non-indexed, non-instanced primitives. 
    /// All queued compute shader dispatch requests are also processed</summary>
    /// <param name="shader">The <see cref="Shader"/> to apply when drawing.</param>
    /// <param name="vertexCount">The number of vertices to draw from the provided vertex buffer(s).</param>
    /// <param name="vertexStartIndex">The vertex to start drawing from.</param>
    public abstract GpuBindResult Draw(Shader shader, uint vertexCount, uint vertexStartIndex = 0);

    /// <summary>Draw instanced, unindexed primitives. </summary>
    /// <param name="shader">The <see cref="Shader"/> to apply when drawing.</param>
    /// <param name="vertexCountPerInstance">The expected number of vertices per instance.</param>
    /// <param name="instanceCount">The expected number of instances.</param>
    /// <param name="vertexStartIndex">The index of the first vertex.</param>
    /// <param name="instanceStartIndex">The index of the first instance element</param>
    public abstract GpuBindResult DrawInstanced(Shader shader,
        uint vertexCountPerInstance,
        uint instanceCount,
        uint vertexStartIndex = 0,
        uint instanceStartIndex = 0);

    /// <summary>Draw indexed, non-instanced primitives.</summary>
    /// <param name="shader">The <see cref="Shader"/> to apply when drawing.</param>
    /// <param name="vertexIndexOffset">A value added to each index before reading from the vertex buffer.</param>
    /// <param name="indexCount">The number of indices to be drawn.</param>
    /// <param name="startIndex">The index to start drawing from.</param>
    public abstract GpuBindResult DrawIndexed(Shader shader,
        uint indexCount,
        int vertexIndexOffset = 0,
        uint startIndex = 0);

    /// <summary>Draw indexed, instanced primitives.</summary>
    /// <param name="shader">The <see cref="Shader"/> to apply when drawing.</param>
    /// <param name="indexCountPerInstance">The expected number of indices per instance.</param>
    /// <param name="instanceCount">The expected number of instances.</param>
    /// <param name="startIndex">The start index.</param>
    /// <param name="vertexIndexOffset">The index of the first vertex.</param>
    /// <param name="instanceStartIndex">The index of the first instance element</param>
    public abstract GpuBindResult DrawIndexedInstanced(Shader shader,
        uint indexCountPerInstance,
        uint instanceCount,
        uint startIndex = 0,
        int vertexIndexOffset = 0,
        uint instanceStartIndex = 0);

    /// <summary>
    /// Dispatches a <see cref="Shader"/> as a compute shader. Any non-compute passes will be skipped.
    /// </summary>
    /// <param name="shader">The shader to be dispatched.</param>
    /// <param name="groups">The number of thread groups.</param>
    /// <returns></returns>
    public abstract GpuBindResult Dispatch(Shader shader, Vector3UI groups);

    protected GpuBindResult Validate(QueueValidationMode mode)
    {
        GpuBindResult result = GpuBindResult.Successful;

        result |= CheckShader();

        // Validate and update mode-specific data if needed.
        switch (mode)
        {
            case QueueValidationMode.Indexed:
                result |= CheckVertexSegment();
                result |= CheckIndexSegment();
                break;

            case QueueValidationMode.Instanced:
                result |= CheckVertexSegment();
                result |= CheckInstancing();
                break;

            case QueueValidationMode.InstancedIndexed:
                result |= CheckVertexSegment();
                result |= CheckIndexSegment();
                result |= CheckInstancing();
                break;

            case QueueValidationMode.Compute:
                result |= CheckComputeGroups();
                break;
        }

        return result;
    }

    protected abstract GpuBindResult CheckInstancing();

    /// <summary>Validate vertex buffer and vertex shader.</summary>
    /// <returns></returns>
    private GpuBindResult CheckShader()
    {
        GpuBindResult result = GpuBindResult.Successful;

        if (State.Shader == null)
            result |= GpuBindResult.MissingMaterial;

        return result;
    }

    private GpuBindResult CheckComputeGroups()
    {
        ComputeCapabilities comCap = Device.Capabilities.Compute;
        Vector3UI groups = DrawInfo.ComputeGroups;

        if (groups.Z > comCap.MaxGroupCountZ)
        {
            Device.Log.Error($"Unable to dispatch compute shader. Z dimension ({groups.Z}) is greater than supported ({comCap.MaxGroupCountZ}).");
            return GpuBindResult.InvalidComputeGroupDimension;
        }

        if (groups.X > comCap.MaxGroupCountX)
        {
            Device.Log.Error($"Unable to dispatch compute shader. X dimension ({groups.X}) is greater than supported ({comCap.MaxGroupCountX}).");
            return GpuBindResult.InvalidComputeGroupDimension;
        }

        if (groups.Y > comCap.MaxGroupCountY)
        {
            Device.Log.Error($"Unable to dispatch compute shader. Y dimension ({groups.Y}) is greater than supported ({comCap.MaxGroupCountY}).");
            return GpuBindResult.InvalidComputeGroupDimension;
        }

        return GpuBindResult.Successful;
    }

    private GpuBindResult CheckVertexSegment()
    {
        GpuBindResult result = GpuBindResult.Successful;

        if (State.VertexBuffers[0] == null)
            result |= GpuBindResult.MissingVertexSegment;

        return result;
    }

    private GpuBindResult CheckIndexSegment()
    {
        GpuBindResult result = GpuBindResult.Successful;

        // If the index buffer is null, this method will always fail because 
        // it assumes it is only being called during an indexed draw call.
        if (State.IndexBuffer.BoundValue == null)
            result |= GpuBindResult.MissingIndexSegment;

        return result;
    }

    /// <summary>
    /// Maps a resource to provide a <see cref="GpuStream"/> for reading or writing.
    /// </summary>
    /// <param name="resource">The resource to be mapped.</param>
    /// <param name="subresource">The sub-resource to be mapped. e.g. mip-map level or array slice.</param>
    /// <param name="offsetBytes">The number of bytes to offset the mapping. This sets the position of the returned <see cref="GpuStream"/>.</param>
    /// <param name="mapType">The type of mapping to perform.</param>
    /// <returns></returns>
    /// <exception cref="GpuResourceException"></exception>
    public unsafe GpuStream MapResource(GpuResource resource, uint subresource, ulong offsetBytes, GpuMapType mapType)
    {
        resource.Apply(this);
        GpuResourceMap map = GetResourcePtr(resource, subresource, mapType);

        if (map.Ptr == null)
            throw new GpuResourceException(resource, "Failed to map resource.");

        GpuStream stream = new GpuStream(this, resource, ref map);
        stream.Position = (long)offsetBytes;
        return stream;
    }

    internal void UnmapResource(GpuStream stream)
    {
        OnUnmapResource(stream.Resource, stream.SubResourceIndex);
    }

    /// <summary>
    /// Invoked when a <see cref="GpuResource"/> is mapped for reading or writing by the CPU/system.
    /// </summary>
    /// <param name="resource">The <see cref="GpuResource"/></param>
    /// <param name="subresource">The sub-resource index. e.g. a texture mip-map level, or array slice.</param>
    /// <param name="mapType">The type of mapping to perform.</param>
    /// <returns></returns>
    protected abstract GpuResourceMap GetResourcePtr(GpuResource resource, uint subresource, GpuMapType mapType);

    protected abstract void OnUnmapResource(GpuResource resource, uint subresource);

    protected internal abstract void CopyResource(GpuResource src, GpuResource dest);

    protected internal abstract unsafe void UpdateResource(GpuResource resource, uint subresource, ResourceRegion? region, void* ptrData, ulong rowPitch, ulong slicePitch);

    public abstract unsafe void CopyResourceRegion(GpuResource source, uint srcSubresource, ResourceRegion? sourceRegion,
        GpuResource dest, uint destSubresource, Vector3UI destStart);

    protected abstract void OnResetState();

    /// <summary>
    /// Releases the current <see cref="GpuCommandList"/> ror recycling and reuse.
    /// </summary>
    public abstract void Free();

    /// <summary>
    /// Starts a new event. Must be paired with a call to <see cref="EndEvent()"/> once finished. Events can aid debugging using the API's debugging toolset, if available.
    /// </summary>
    public abstract void BeginEvent(string label);

    /// <summary>
    /// Ends an event that was started with <see cref="BeginEvent(string)"/>. Events can aid debugging using the API's debugging toolset, if available.
    /// </summary>
    public abstract void EndEvent();

    /// <summary>
    /// Sets an API marker (if supported), to aid the use of the API's debugging toolset.
    /// </summary>
    public abstract void SetMarker(string label);

    public uint BranchIndex { get; set; }

    public GpuCommandListFlags Flags { get; set; }

    public GpuCommandList Previous { get; internal set; }

    /// <summary>Gets the profiler bound to the current <see cref="GpuCommandList"/>.</summary>
    public GraphicsQueueProfiler Profiler { get; }

    protected BatchDrawInfo DrawInfo { get; }

    /// <summary>
    /// Gets GPU fence that can be used to synchronize the completed execution of the current <see cref="GpuCommandList"/> with application/CPU-based operations.
    /// </summary>
    public abstract GpuFence Fence { get; }

    /// <summary>
    /// Gets whether or not <see cref="Begin()"/> has been called.
    /// </summary>
    public bool HasBegan => DrawInfo.Began;

    /// <summary>
    /// Gets the pipeline state of the current <see cref="GpuCommandQueue"/>.
    /// </summary>
    public GpuState State => _state;
}
