using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.Maths;

namespace Molten.Graphics.DX12;

public unsafe class CommandListDX12 : GpuCommandList
{
    bool _isClosed;
    ID3D12GraphicsCommandList* _handle;
    PipelineInputLayoutDX12 _inputLayout;
    PipelineStateDX12 _pipelineState;

    CpuDescriptorHandle* _rtvs;
    CpuDescriptorHandle* _dsv;
    uint _numRTVs;

    D3DPrimitiveTopology _boundTopology;
    GpuDepthWritePermission _boundDepthMode;


    internal CommandListDX12(CommandAllocatorDX12 allocator, ID3D12GraphicsCommandList* handle) :
        base(allocator.Device)
    {
        Device = allocator.Device;
        Allocator = allocator;
        Fence = new FenceDX12(allocator.Device, FenceFlags.None);

        uint maxRTs = Device.Capabilities.PixelShader.MaxOutputTargets;
        _rtvs = EngineUtil.AllocArray<CpuDescriptorHandle>(maxRTs);
        _dsv = EngineUtil.AllocArray<CpuDescriptorHandle>(1);
        _dsv[0] = default;

        Close();
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
        if (resource.Handle is ResourceHandleDX12 handle)
        {
            ulong rowPitch = 0;
            ulong depthPitch = 0;

            if (resource is GpuTexture tex)
            {
                // TODO Calculate row pitch based on texture size, subresource level, format and dimensions. Also consider block-compression size.
            }
            else if (resource is GpuBuffer buffer)
            {
                rowPitch = buffer.SizeInBytes;
                depthPitch = buffer.SizeInBytes;
            }

            void* ptrMap = null;
            HResult hr = handle.Ptr1->Map(subresource, null, &ptrMap);
            if (!Device.Log.CheckResult(hr, () => $"Failed to map resource {resource.Name} for {mapType} access"))
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

    protected override void CopyResource(GpuResource src, GpuResource dest)
    {
        src.Apply(this);
        dest.Apply(this);

        _handle->CopyResource((ResourceHandleDX12)dest.Handle, (ResourceHandleDX12)src.Handle);
        Profiler.ResourceCopyCalls++;
    }

    public override void Begin()
    {
        base.Begin();

        ID3D12PipelineState* pState = null; // initialState != null ? initialState.Handle : null; // TODO Add initial state support
        Handle->Reset(Device.CommandAllocator.Handle, pState);
        _isClosed = false;
    }

    private void Close()
    {
        if (!_isClosed)
        {
            _handle->Close();
            _isClosed = true;
        }
    }

    public override void Execute(GpuCommandList cmd)
    {
        CommandListDX12 dxCmd = (CommandListDX12)cmd;
        if (dxCmd.Type != CommandListType.Bundle)
            throw new GpuCommandListException(this, "Cannot execute a non-bundle command list on another command list");

        _handle->ExecuteBundle(dxCmd.Handle);
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

    protected override void OnGenerateMipmaps(GpuTexture texture)
    {
        // TODO: Implement compute-based mip-map generation - This can then be commonized for DX11/Vulkan too.
        //       See: https://www.3dgep.com/learning-directx-12-4/#Generate_Mipmaps_Compute_Shader

        throw new NotImplementedException();
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
        _handle->ResourceBarrier(1, &barrier);
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
        _handle->ResourceBarrier(1, &barrier);
    }

    protected override void OnResetState()
    {
        // Unbind all output surfaces
        _handle->OMSetRenderTargets(0, null, false, null);
    }

    protected override GpuBindResult DoComputePass(ShaderPass pass)
    {
        throw new NotImplementedException();
    }

    protected override GpuBindResult CheckInstancing()
    {
        if (_inputLayout != null && _inputLayout.IsInstanced)
            return GpuBindResult.Successful;
        else
            return GpuBindResult.NonInstancedVertexLayout;
    }

    private void BindVertexBuffers()
    {
        int count = State.VertexBuffers.Length;
        VertexBufferView* pBuffers = stackalloc VertexBufferView[count];
        GpuBuffer buffer = null;

        for (int i = 0; i < count; i++)
        {
            buffer = State.VertexBuffers.BoundValues[i];

            if (buffer != null)
                pBuffers[i] = ((VBHandleDX12)buffer.Handle).View;
            else
                pBuffers[i] = default;
        }

        _handle->IASetVertexBuffers(0, (uint)count, pBuffers);
    }

    internal void ClearDSV(TextureDX12 surface, Color color)
    {
        if (surface.Handle is RTHandleDX12 rtHandle)
        {
            Transition(surface, ResourceStates.RenderTarget);
            ref CpuDescriptorHandle cpuHandle = ref rtHandle.RTV.CpuHandle;
            Color4 c4 = color.ToColor4();

            _handle->ClearRenderTargetView(cpuHandle, c4.Values, 0, null);
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
        if (flags > 0)
            _handle->ClearDepthStencilView(cpuHandle, flags, depthValue, stencilValue, 0, null);
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

    public override unsafe void CopyResourceRegion(GpuResource source, uint srcSubresource, ResourceRegion? sourceRegion, GpuResource dest, uint destSubresource, Vector3UI destStart)
    {
        throw new NotImplementedException();
    }

    public override GpuBindResult Draw(Shader shader, uint vertexCount, uint vertexStartIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.Unindexed, () =>
            _handle->DrawInstanced(vertexCount, 1, vertexStartIndex, 0));
    }

    public override GpuBindResult DrawInstanced(Shader shader, uint vertexCountPerInstance,
        uint instanceCount,
        uint vertexStartIndex = 0,
        uint instanceStartIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.Instanced, () =>
            _handle->DrawInstanced(vertexCountPerInstance, instanceCount, vertexStartIndex, instanceStartIndex));
    }

    public override GpuBindResult DrawIndexed(Shader shader, uint indexCount, int vertexIndexOffset = 0, uint startIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.Indexed, () =>
            _handle->DrawIndexedInstanced(indexCount, 1, startIndex, vertexIndexOffset, 0));
    }

    public override GpuBindResult DrawIndexedInstanced(Shader shader, uint indexCountPerInstance, uint instanceCount, uint startIndex = 0, int vertexIndexOffset = 0, uint instanceStartIndex = 0)
    {
        return ApplyState(shader, QueueValidationMode.InstancedIndexed, () =>
            _handle->DrawIndexedInstanced(indexCountPerInstance, instanceCount, startIndex, vertexIndexOffset, instanceStartIndex));
    }

    public override GpuBindResult Dispatch(Shader shader, Vector3UI groups)
    {
        DrawInfo.Custom.ComputeGroups = groups;
        return ApplyState(shader, QueueValidationMode.Compute, null);
    }

    protected override GpuBindResult DoRenderPass(ShaderPass hlslPass, QueueValidationMode mode, Action callback)
    {
        ShaderPassDX12 pass = hlslPass as ShaderPassDX12;
        D3DPrimitiveTopology passTopology = pass.Topology.ToApi();

        if (passTopology == D3DPrimitiveTopology.D3D11PrimitiveTopologyUndefined)
            return GpuBindResult.UndefinedTopology;

        // Clear output merger and rebind targets later.
        _handle->OMSetRenderTargets(0, null, false, null);

        // Check topology
        if (_boundTopology != passTopology)
        {
            _boundTopology = passTopology;
            _handle->IASetPrimitiveTopology(_boundTopology);
        }

        if (State.VertexBuffers.Bind(this))
            BindVertexBuffers();

        if (State.IndexBuffer.Bind(this))
        {
            GpuBuffer iBuffer = State.IndexBuffer.BoundValue;
            if (iBuffer != null)
            {
                IBHandleDX12 ibHandle = (IBHandleDX12)iBuffer.Handle;
                _handle->IASetIndexBuffer(ibHandle.View);
            }
            else
            {
                _handle->IASetIndexBuffer(null);
            }
        }

        // Check if viewports need updating.
        // TODO Consolidate - Molten viewports are identical in memory layout to DX11 viewports.
        if (State.Viewports.IsDirty)
        {
            fixed (ViewportF* ptrViewports = State.Viewports.Items)
                _handle->RSSetViewports((uint)State.Viewports.Length, (Silk.NET.Direct3D12.Viewport*)ptrViewports);

            State.Viewports.IsDirty = false;
        }

        // Check if scissor rects need updating
        if (State.ScissorRects.IsDirty)
        {
            fixed (Rectangle* ptrRect = State.ScissorRects.Items)
                _handle->RSSetScissorRects((uint)State.ScissorRects.Length, (Box2D<int>*)ptrRect);

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
        PipelineStateDX12 state = Device.StateBuilder.Build(pass, _inputLayout);

        _handle->SetPipelineState(state.Handle);
        _handle->SetGraphicsRootSignature(state.RootSignature.Handle);

        Device.Heap.PrepareGpuHeap(pass, this);

        CpuDescriptorHandle* dsvHandle = _dsv->Ptr != 0 ? _dsv : null;
        _handle->OMSetRenderTargets(_numRTVs, _rtvs, false, dsvHandle);
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

    /// <summary>Retrieves or creates a usable input layout for the provided vertex buffers and sub-effect.</summary>
    /// <returns>An instance of InputLayout.</returns>
    private PipelineInputLayoutDX12 GetInputLayout(ShaderPassDX12 pass)
    {
        // Retrieve layout list or create new one if needed.
        PipelineInputLayoutDX12 match = null;

        Device.PipelineLayoutCache.For(0, (index, layout) =>
        {
            if (layout.IsMatch(State.VertexBuffers))
            {
                match = layout;
                return true;
            }

            return false;
        });

        if (match != null)
            return match;

        PipelineInputLayoutDX12 input = new PipelineInputLayoutDX12(Device, State.VertexBuffers, pass);
        Device.PipelineLayoutCache.Add(input);

        return input;
    }

    public override void Free()
    {
        throw new NotImplementedException();
    }

    protected override void OnGraphicsRelease()
    {
        EngineUtil.Free(ref _rtvs);
        EngineUtil.Free(ref _dsv);
        NativeUtil.ReleasePtr(ref _handle);
        Fence?.Dispose();

        Allocator.Unallocate(this);
    }

    /// <summary>
    /// Gets the parent <see cref="CommandAllocatorDX12"/> from which the current <see cref="CommandListDX12"/> was allocated.
    /// </summary>
    internal CommandAllocatorDX12 Allocator { get; }

    public CommandListType Type => Allocator.Type;

    public override FenceDX12 Fence { get; }

    public static implicit operator ID3D12CommandList*(CommandListDX12 cmd) => (ID3D12CommandList*)cmd._handle;

    public unsafe ID3D12CommandList* BaseHandle => (ID3D12CommandList*)_handle;

    internal ref readonly ID3D12GraphicsCommandList* Handle => ref _handle;

    public new DeviceDX12 Device { get; }
}
