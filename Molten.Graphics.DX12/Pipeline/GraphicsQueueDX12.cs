using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

public unsafe class GraphicsQueueDX12 : GraphicsQueue<DeviceDX12>
{
    CommandQueueDesc _desc;
    ID3D12CommandQueue* _handle;
    PipelineInputLayoutDX12 _inputLayout;
    PipelineStateDX12 _pipelineState;
    List<PipelineInputLayoutDX12> _cachedLayouts = new List<PipelineInputLayoutDX12>();

    GraphicsCommandListDX12 _cmd;
    GraphicsFrameBuffer<CommandAllocatorDX12> _cmdAllocators;

    internal GraphicsQueueDX12(Logger log, DeviceDX12 device, DeviceBuilderDX12 builder, ref CommandQueueDesc desc) : 
        base(device)
    {
        _desc = desc;
        Log = log;
        Device = device;

        Initialize(builder);
    }

    private void Initialize(DeviceBuilderDX12 builder)
    {
        Guid cmdGuid = ID3D12CommandQueue.Guid;
        void* cmdQueue = null;

        DeviceDX12 device = Device; 
        HResult r = device.Handle->CreateCommandQueue(_desc, &cmdGuid, &cmdQueue);
        if (!device.Log.CheckResult(r))
        {
            Log.Error($"Failed to initialize '{_desc.Type}' command queue");
            return;
        }
        else
        {
            Log.WriteLine($"Initialized '{_desc.Type}' command queue");
        }

        _handle = (ID3D12CommandQueue*)cmdQueue;
        _cmdAllocators = new GraphicsFrameBuffer<CommandAllocatorDX12>(Device, (device) =>
        {
            return new CommandAllocatorDX12(this, CommandListType.Direct);
        });
    }

    protected override void GenerateMipMaps(GraphicsResource texture)
    {
        // TODO: Implement compute-based mip-map generation - This can then be commonized for DX11/Vulkan too.
        //       See: https://www.3dgep.com/learning-directx-12-4/#Generate_Mipmaps_Compute_Shader

        throw new NotImplementedException();
    }

    internal void Clear(TextureDX12 surface, Color color)
    {
        if (surface.Handle is RTHandleDX12 rtHandle)
        {
            Transition(surface, ResourceStates.RenderTarget);
            ref CpuDescriptorHandle cpuHandle = ref rtHandle.RTV.CpuHandle;
            Color4 c4 = color.ToColor4();

            _cmd.Handle->ClearRenderTargetView(cpuHandle, c4.Values, 0, null);
        }
        else
        {
            throw new GraphicsResourceException(surface, "Cannot clear a non-render surface texture.");
        }
    }

    internal void Clear(DepthSurfaceDX12 surface, float depthValue, byte stencilValue)
    {
        Transition(surface, ResourceStates.DepthWrite);

        DSHandleDX12 dsHandle = (DSHandleDX12)surface.Handle;
        ref CpuDescriptorHandle cpuHandle = ref dsHandle.DSV.CpuHandle;
        ClearFlags flags = ClearFlags.Depth;
        if (surface.DepthFormat.HasStencil())
            flags |= ClearFlags.Stencil;

        // TODO Add support for clearing areas using Box2D structs.
        _cmd.Handle->ClearDepthStencilView(cpuHandle, flags, depthValue, stencilValue, 0, null);
    }

    internal void Transition(BufferDX12 buffer, ResourceStates newState)
    {
        ResourceBarrier barrier = new()
        {
            Flags = ResourceBarrierFlags.None,
            Type = ResourceBarrierType.Transition,
            Transition = new ResourceTransitionBarrier()
            {
                PResource = buffer.Handle,
                StateAfter = newState,
                StateBefore = buffer.BarrierState,
                Subresource = 0,
            },
        };        

        _cmd.Handle->ResourceBarrier(1, &barrier);
    }

    internal void Transition(TextureDX12 texture, ResourceStates newState)
    {
        if (texture.BarrierState == newState)
            return;

        ResourceBarrier barrier = new()
        {
            Flags = ResourceBarrierFlags.None,
            Type = ResourceBarrierType.Transition,
            Transition = new ResourceTransitionBarrier()
            {
                PResource = texture.Handle,
                StateAfter = newState,
                StateBefore = texture.BarrierState,
                Subresource = 0,
            },
        };

        _cmd.Handle->ResourceBarrier(1, &barrier);
    }

    protected override void OnResetState()
    {
        throw new NotImplementedException();
    }

    public override void Execute(GraphicsCommandList list)
    {
        GraphicsCommandListDX12 cmd = (GraphicsCommandListDX12)list;

        cmd.Close();
        ID3D12CommandList** lists = stackalloc ID3D12CommandList*[] { cmd.BaseHandle };
        _handle->ExecuteCommandLists(1, lists);
    }

    public override void Sync(GraphicsCommandListFlags flags)
    {
        if (flags.Has(GraphicsCommandListFlags.Deferred))
            throw new InvalidOperationException($"An immediate/primary command list branch cannot use deferred flag during Sync() calls.");

        Execute(_cmd);

        // A fence will signal a synchronization event.
        // This blocks the CPU until the GPU has finished processing all commands prior to the fences signal command.
        if (!_cmd.Fence.Wait())
            throw new InvalidOperationException("Command list Sync() fence failed Wait() call. See logs for details");

        CommandAllocatorDX12 allocator = _cmdAllocators.Prepare();
        _cmd.Reset(allocator, _pipelineState);
    }

    public override void Begin(GraphicsCommandListFlags flags = GraphicsCommandListFlags.None)
    {
        base.Begin(flags);

        CommandAllocatorDX12 allocator = _cmdAllocators.Prepare();
        if (_cmd == null || _cmd.Flags.HasFlag(GraphicsCommandListFlags.Deferred))
        {
            _cmd = allocator.Allocate(null);
            Device.Frame.BranchCount++;
            Device.Frame.Track(_cmd);
        }

        _cmd.Reset(allocator, _pipelineState);
    }

    public override GraphicsCommandList End()
    {
        base.End();

        if (_cmd.Flags.HasFlag(GraphicsCommandListFlags.Deferred))
            return _cmd;

        Execute(_cmd);
        _cmd.Fence.Wait();

        return null;
    }

    public override void BeginEvent(string label)
    {
        // TODO Requires mappings for PIX on Windows: https://devblogs.microsoft.com/pix/winpixeventruntime/
        // See Also: https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/reference/tools/pix3/functions/pixscopedevent-overloads
    }

    public override void EndEvent()
    {
        // TODO Requires mappings for PIX on Windows: https://devblogs.microsoft.com/pix/winpixeventruntime/
        // See Also: https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/reference/tools/pix3/functions/pixscopedevent-overloads
    }

    public override void SetMarker(string label)
    {
        // TODO Requires mappings for PIX on Windows: https://devblogs.microsoft.com/pix/winpixeventruntime/
        // See Also: https://learn.microsoft.com/en-us/gaming/gdk/_content/gc/reference/tools/pix3/functions/pixscopedevent-overloads
    }

    protected override GraphicsBindResult DoRenderPass(ShaderPass pass, QueueValidationMode mode, Action callback)
    {
        throw new NotImplementedException();
    }

    protected override GraphicsBindResult DoComputePass(ShaderPass pass)
    {
        throw new NotImplementedException();
    }

    protected override ResourceMap GetResourcePtr(GraphicsResource resource, uint subresource, GraphicsMapType mapType)
    {
        GraphicsResourceFlags flags = resource.Flags;

        // Validate map type.
        if (mapType == GraphicsMapType.Read)
        {
            if (!flags.Has(GraphicsResourceFlags.CpuRead))
                throw new InvalidOperationException($"Resource '{resource.Name}' does not allow read access.");
        }
        else if (mapType == GraphicsMapType.Write)
        {
            if (!flags.Has(GraphicsResourceFlags.CpuWrite))
                throw new InvalidOperationException($"Resource '{resource.Name}' does not allow write access.");
        }
        else if (mapType == GraphicsMapType.Discard)
        {
            // TODO Validate this.
        }

        if(resource.Handle is ResourceHandleDX12 handle)
        {
            ulong rowPitch = 0;
            ulong depthPitch = 0;

            if (resource is GraphicsTexture tex)
            {
                // TODO Calculate row pitch based on texture size, subresource level, format and dimensions. Also consider block-compression size.
            }
            else if (resource is GraphicsBuffer buffer)
            {
                rowPitch = buffer.SizeInBytes;
                depthPitch = buffer.SizeInBytes;
            }

            void* ptrMap = null;
            HResult hr = handle.Ptr1->Map(subresource, null, &ptrMap);
            if (!Log.CheckResult(hr, () => $"Failed to map resource {resource.Name} for {mapType} access"))
                return new ResourceMap();
            else
                return new ResourceMap(ptrMap, rowPitch, depthPitch);
        }
        else
        {
            throw new InvalidOperationException($"Resource '{resource.Name}' is not a valid DX12 resource.");
        }        
    }

    protected override void OnUnmapResource(GraphicsResource resource, uint subresource)
    {
        if (resource.Handle is ResourceHandleDX12 handle)
            handle.Ptr1->Unmap(subresource, null);
    }

    protected override unsafe void UpdateResource(GraphicsResource resource, uint subresource, ResourceRegion? region, void* ptrData, uint rowPitch, uint slicePitch)
    {
        throw new NotImplementedException();
    }

    protected override void CopyResource(GraphicsResource src, GraphicsResource dest)
    {
        src.Apply(this);
        dest.Apply(this);

        _cmd.CopyResource(dest, src);
        Profiler.ResourceCopyCalls++;
    }

    public override unsafe void CopyResourceRegion(GraphicsResource source, uint srcSubresource, ResourceRegion? sourceRegion, GraphicsResource dest, uint destSubresource, Vector3UI destStart)
    {
        throw new NotImplementedException();
    }

    public override GraphicsBindResult Draw(Shader shader, uint vertexCount, uint vertexStartIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.Unindexed, () =>
            _cmd.Handle->DrawInstanced(vertexCount, 1, vertexStartIndex, 0));
    }

    public override GraphicsBindResult DrawInstanced(Shader shader, uint vertexCountPerInstance,
        uint instanceCount,
        uint vertexStartIndex = 0,
        uint instanceStartIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.Instanced, () =>
            _cmd.Handle->DrawInstanced(vertexCountPerInstance, instanceCount, vertexStartIndex, instanceStartIndex));
    }

    public override GraphicsBindResult DrawIndexed(Shader shader, uint indexCount, int vertexIndexOffset = 0, uint startIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.Indexed, () =>
            _cmd.Handle->DrawIndexedInstanced(indexCount, 1, startIndex, vertexIndexOffset, 0));
    }

    public override GraphicsBindResult DrawIndexedInstanced(Shader shader, uint indexCountPerInstance, uint instanceCount, uint startIndex = 0, int vertexIndexOffset = 0, uint instanceStartIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.InstancedIndexed, () =>
            _cmd.Handle->DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndex, vertexIndexOffset, instanceStartIndex));
    }

    public override GraphicsBindResult Dispatch(Shader shader, Vector3UI groups)
    {
        DrawInfo.Custom.ComputeGroups = groups;
        return ApplyState(shader, QueueValidationMode.Compute, null);
    }

    protected override void OnDispose(bool immediate)
    {
        _cmdAllocators?.Dispose(true);
        NativeUtil.ReleasePtr(ref _handle);
    }

    protected override GraphicsBindResult CheckInstancing()
    {
        if (_inputLayout != null && _inputLayout.IsInstanced)
            return GraphicsBindResult.Successful;
        else
            return GraphicsBindResult.NonInstancedVertexLayout;
    }

    /// <summary>Retrieves or creates a usable input layout for the provided vertex buffers and sub-effect.</summary>
    /// <returns>An instance of InputLayout.</returns>
    private PipelineInputLayoutDX12 GetInputLayout(ShaderPassDX12 pass)
    {
        // Retrieve layout list or create new one if needed.
        foreach (PipelineInputLayoutDX12 l in _cachedLayouts)
        {
            if (l.IsMatch(State.VertexBuffers))
                return l;
        }

        PipelineInputLayoutDX12 input = new PipelineInputLayoutDX12(Device, State.VertexBuffers, pass);
        _cachedLayouts.Add(input);

        return input;
    }

    internal ID3D12CommandQueue* Handle => _handle;

    internal Logger Log { get; }

    protected override CommandListDX12 Cmd => _cmd;

    internal new DeviceDX12 Device { get; }
}
