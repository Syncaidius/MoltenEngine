using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.Maths;

namespace Molten.Graphics.DX12;

public unsafe class GraphicsQueueDX12 : GraphicsQueue<DeviceDX12>
{
    CommandQueueDesc _desc;
    ID3D12CommandQueue* _handle;
    PipelineInputLayoutDX12 _inputLayout;
    PipelineStateDX12 _pipelineState;
    List<PipelineInputLayoutDX12> _cachedLayouts = new List<PipelineInputLayoutDX12>();

    GraphicsCommandListDX12 _cmd;
    GpuFrameBuffer<CommandAllocatorDX12> _cmdAllocators;
    PipelineStateBuilderDX12 _stateBuilder;
        
    CpuDescriptorHandle* _rtvs;
    CpuDescriptorHandle* _dsv;
    uint _numRTVs;

    D3DPrimitiveTopology _boundTopology;
    GpuDepthWritePermission _boundDepthMode;

    internal GraphicsQueueDX12(Logger log, DeviceDX12 device, DeviceBuilderDX12 builder, ref CommandQueueDesc desc) : 
        base(device)
    {
        _desc = desc;
        Log = log;
        Device = device;

        uint maxRTs = Device.Capabilities.PixelShader.MaxOutputTargets;
        _rtvs = EngineUtil.AllocArray<CpuDescriptorHandle>(maxRTs);
        _dsv = EngineUtil.AllocArray<CpuDescriptorHandle>(1);
        _dsv[0] = default;

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

        Log.WriteLine($"Initialized '{_desc.Type}' command queue");

        _handle = (ID3D12CommandQueue*)cmdQueue;
        _stateBuilder = new PipelineStateBuilderDX12(device);
        _cmdAllocators = new GpuFrameBuffer<CommandAllocatorDX12>(Device, (device) =>
        {
            return new CommandAllocatorDX12(this, CommandListType.Direct);
        });
    }

    protected override void GenerateMipMaps(GpuResource texture)
    {
        // TODO: Implement compute-based mip-map generation - This can then be commonized for DX11/Vulkan too.
        //       See: https://www.3dgep.com/learning-directx-12-4/#Generate_Mipmaps_Compute_Shader

        throw new NotImplementedException();
    }

    internal void ClearDSV(TextureDX12 surface, Color color)
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
            throw new GpuResourceException(surface, "Cannot clear a non-render surface texture.");
        }
    }

    internal void Clear(DepthSurfaceDX12 surface, float depthValue, byte stencilValue, DepthClearFlags clearFlags)
    {
        Transition(surface, ResourceStates.DepthWrite);

        DSHandleDX12 dsHandle = (DSHandleDX12)surface.Handle;
        ref CpuDescriptorHandle cpuHandle = ref dsHandle.DSV.CpuHandle;
        ClearFlags flags = 0;

        if (clearFlags.Has(DepthClearFlags.Depth))
            flags = ClearFlags.Depth;

        if (surface.DepthFormat.HasStencil() && clearFlags.HasFlag(DepthClearFlags.Stencil))
            flags |= ClearFlags.Stencil;

        // TODO Add support for clearing areas using Box2D structs.
        if(flags > 0)
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

        buffer.BarrierState = newState;
        _cmd.Handle->ResourceBarrier(1, &barrier);
    }

    internal void Transition(TextureDX12 texture, ResourceStates newState, uint subResource = 0)
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
                Subresource = subResource,
            },
        };

        texture.BarrierState = newState;
        _cmd.Handle->ResourceBarrier(1, &barrier);
    }

    protected override void OnResetState()
    {
        // Unbind all output surfaces
        _cmd.Handle->OMSetRenderTargets(0, null, false, null);
    }

    public override void Execute(GpuCommandList list)
    {
        GraphicsCommandListDX12 cmd = (GraphicsCommandListDX12)list;

        cmd.Close();
        ID3D12CommandList** lists = stackalloc ID3D12CommandList*[] { cmd.BaseHandle };
        _handle->ExecuteCommandLists(1, lists);
    }

    public override void Sync(GpuCommandListFlags flags)
    {
        if (flags.Has(GpuCommandListFlags.Deferred))
            throw new InvalidOperationException($"An immediate/primary command list branch cannot use deferred flag during Sync() calls.");

        Execute(_cmd);

        // A fence will signal a synchronization event.
        // This blocks the CPU until the GPU has finished processing all commands prior to the fences signal command.
        if (!_cmd.Fence.Wait())
            throw new InvalidOperationException("Command list Sync() fence failed Wait() call. See logs for details");

        CommandAllocatorDX12 allocator = _cmdAllocators.Prepare();
        _cmd.Reset(allocator, _pipelineState);
    }

    public override void Begin(GpuCommandListFlags flags = GpuCommandListFlags.None)
    {
        base.Begin(flags);

        CommandAllocatorDX12 allocator = _cmdAllocators.Prepare();
        if (_cmd == null || _cmd.Flags.HasFlag(GpuCommandListFlags.Deferred))
        {
            _cmd = allocator.Allocate(null);
            Device.Frame.BranchCount++;
            //Device.Frame.Track(_cmd);
        }

        _cmd.Reset(allocator, _pipelineState);
    }

    public override GpuCommandList End()
    {
        base.End();

        if (_cmd.Flags.HasFlag(GpuCommandListFlags.Deferred))
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

    protected override GpuBindResult DoRenderPass(ShaderPass hlslPass, QueueValidationMode mode, Action callback)
    {
        ShaderPassDX12 pass = hlslPass as ShaderPassDX12;
        D3DPrimitiveTopology passTopology = pass.Topology.ToApi();

        if (passTopology == D3DPrimitiveTopology.D3D11PrimitiveTopologyUndefined)
            return GpuBindResult.UndefinedTopology;

        // Clear output merger and rebind targets later.
        _cmd.Handle->OMSetRenderTargets(0, null, false, null);

        // Check topology
        if (_boundTopology != passTopology)
        {
            _boundTopology = passTopology;
            _cmd.Handle->IASetPrimitiveTopology(_boundTopology);
        }
 
        if (State.VertexBuffers.Bind(this))
            BindVertexBuffers();

        if (State.IndexBuffer.Bind(this))
        {
            GraphicsBuffer iBuffer = State.IndexBuffer.BoundValue;
            if (iBuffer != null)
            {
                IBHandleDX12 ibHandle = (IBHandleDX12)iBuffer.Handle;
                _cmd.Handle->IASetIndexBuffer(ibHandle.View);
            }
            else
            {
                _cmd.Handle->IASetIndexBuffer(null);
            }
        }

        // Check if viewports need updating.
        // TODO Consolidate - Molten viewports are identical in memory layout to DX11 viewports.
        if (State.Viewports.IsDirty)
        {
            fixed (ViewportF* ptrViewports = State.Viewports.Items)
                _cmd.Handle->RSSetViewports((uint)State.Viewports.Length, (Silk.NET.Direct3D12.Viewport*)ptrViewports);

            State.Viewports.IsDirty = false;
        }

        // Check if scissor rects need updating
        if (State.ScissorRects.IsDirty)
        {
            fixed (Rectangle* ptrRect = State.ScissorRects.Items)
                _cmd.Handle->RSSetScissorRects((uint)State.ScissorRects.Length, (Box2D<int>*)ptrRect);

            State.ScissorRects.IsDirty = false;
        }


        if (State.Surfaces.Bind(this))
        {
            _numRTVs = 0;

            for (int i = 0; i < State.Surfaces.Length; i++)
            {
                if (State.Surfaces.BoundValues[i] != null)
                {
                    RTHandleDX12 rsHandle = State.Surfaces.BoundValues[i].Handle as RTHandleDX12;
                    _rtvs[_numRTVs] = rsHandle.RTV.CpuHandle;
                    _numRTVs++;
                }
            }
        }

        GpuDepthWritePermission depthWriteMode = pass.WritePermission;
        if (State.DepthSurface.Bind(this) || (_boundDepthMode != depthWriteMode))
        {
            if (State.DepthSurface.BoundValue != null && depthWriteMode != GpuDepthWritePermission.Disabled)
            {
                DSHandleDX12 dsHandle = State.DepthSurface.BoundValue.Handle as DSHandleDX12;
                if (depthWriteMode == GpuDepthWritePermission.ReadOnly)
                    _dsv[0] = dsHandle.ReadOnlyDSV.CpuHandle;
                else
                    _dsv[0] = dsHandle.DSV.CpuHandle;
            }
            else
            {
                _dsv[0] = default;
            }

            _boundDepthMode = depthWriteMode;
        }

        PipelineInputLayoutDX12 _inputLayout = GetInputLayout(pass);
        PipelineStateDX12 state = _stateBuilder.Build(pass, _inputLayout);

        _cmd.Handle->SetPipelineState(state.Handle);
        _cmd.Handle->SetGraphicsRootSignature(state.RootSignature.Handle);

        Device.Heap.PrepareGpuHeap(pass, _cmd);

        CpuDescriptorHandle* dsvHandle = _dsv->Ptr != 0 ? _dsv : null;
        _cmd.Handle->OMSetRenderTargets(_numRTVs, _rtvs, false, dsvHandle);
        Profiler.BindSurfaceCalls++;

        GpuBindResult vResult = Validate(mode);

        if (vResult == GpuBindResult.Successful)
        {
            // Re-render the same pass for K iterations.
            for (int k = 0; k < pass.Iterations; k++)
            {
                BeginEvent($"Iteration {k}");
                callback();
                Profiler.DrawCalls++;
                EndEvent();
            }
        }

        // Validate pipeline state.
        return vResult;
    }

    protected override GpuBindResult DoComputePass(ShaderPass pass)
    {
        throw new NotImplementedException();
    }

    private void BindVertexBuffers()
    {
        int count = State.VertexBuffers.Length;
        VertexBufferView* pBuffers = stackalloc VertexBufferView[count];
        GraphicsBuffer buffer = null;

        for (int i = 0; i < count; i++)
        {
            buffer = State.VertexBuffers.BoundValues[i];

            if (buffer != null)
                pBuffers[i] = ((VBHandleDX12)buffer.Handle).View;
            else
                pBuffers[i] = default;
        }

        _cmd.Handle->IASetVertexBuffers(0, (uint)count, pBuffers);
    }

    protected override GpuResourceMap GetResourcePtr(GpuResource resource, uint subresource, GpuMapType mapType)
    {
        GpuResourceFlags flags = resource.Flags;

        // Validate map type.
        if (mapType == GpuMapType.Read)
        {
            if (!flags.Has(GpuResourceFlags.CpuRead))
                throw new InvalidOperationException($"Resource '{resource.Name}' does not allow read access.");
        }
        else if (mapType == GpuMapType.Write)
        {
            if (!flags.Has(GpuResourceFlags.CpuWrite))
                throw new InvalidOperationException($"Resource '{resource.Name}' does not allow write access.");
        }
        else if (mapType == GpuMapType.Discard)
        {
            // TODO Validate this.
        }

        resource.Apply(this);
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
                return new GpuResourceMap();
            else
                return new GpuResourceMap(ptrMap, rowPitch, depthPitch);
        }
        else
        {
            throw new InvalidOperationException($"Resource '{resource.Name}' is not a valid DX12 resource.");
        }        
    }

    protected override void OnUnmapResource(GpuResource resource, uint subresource)
    {
        if (resource.Handle is ResourceHandleDX12 handle)
            handle.Ptr1->Unmap(subresource, null);
    }

    protected override unsafe void UpdateResource(GpuResource resource, uint subresource, ResourceRegion? region, void* ptrData, uint rowPitch, uint slicePitch)
    {
        Box* destBox = null;

        if (region != null)
        {
            ResourceRegion value = region.Value;
            destBox = (Box*)&value;
        }
        
        // TODO Calculate byte offset and number of bytes from resource region.

        using (GpuStream stream = MapResource(resource, subresource, 0, GpuMapType.Write))
        {
            stream.Write(ptrData, slicePitch);
            Profiler.SubResourceUpdateCalls++;
        }
    }

    protected override void CopyResource(GpuResource src, GpuResource dest)
    {
        src.Apply(this);
        dest.Apply(this);

        _cmd.CopyResource(dest, src);
        Profiler.ResourceCopyCalls++;
    }

    public override unsafe void CopyResourceRegion(GpuResource source, uint srcSubresource, ResourceRegion? sourceRegion, GpuResource dest, uint destSubresource, Vector3UI destStart)
    {
        throw new NotImplementedException();
    }

    public override GpuBindResult Draw(Shader shader, uint vertexCount, uint vertexStartIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.Unindexed, () =>
            _cmd.Handle->DrawInstanced(vertexCount, 1, vertexStartIndex, 0));
    }

    public override GpuBindResult DrawInstanced(Shader shader, uint vertexCountPerInstance,
        uint instanceCount,
        uint vertexStartIndex = 0,
        uint instanceStartIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.Instanced, () =>
            _cmd.Handle->DrawInstanced(vertexCountPerInstance, instanceCount, vertexStartIndex, instanceStartIndex));
    }

    public override GpuBindResult DrawIndexed(Shader shader, uint indexCount, int vertexIndexOffset = 0, uint startIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.Indexed, () =>
            _cmd.Handle->DrawIndexedInstanced(indexCount, 1, startIndex, vertexIndexOffset, 0));
    }

    public override GpuBindResult DrawIndexedInstanced(Shader shader, uint indexCountPerInstance, uint instanceCount, uint startIndex = 0, int vertexIndexOffset = 0, uint instanceStartIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.InstancedIndexed, () =>
            _cmd.Handle->DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndex, vertexIndexOffset, instanceStartIndex));
    }

    public override GpuBindResult Dispatch(Shader shader, Vector3UI groups)
    {
        DrawInfo.Custom.ComputeGroups = groups;
        return ApplyState(shader, QueueValidationMode.Compute, null);
    }

    protected override void OnDispose(bool immediate)
    {
        _cmdAllocators?.Dispose(true);

        EngineUtil.Free(ref _rtvs);
        EngineUtil.Free(ref _dsv);
        NativeUtil.ReleasePtr(ref _handle);
    }

    protected override GpuBindResult CheckInstancing()
    {
        if (_inputLayout != null && _inputLayout.IsInstanced)
            return GpuBindResult.Successful;
        else
            return GpuBindResult.NonInstancedVertexLayout;
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
