using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.Maths;

namespace Molten.Graphics.DX12;

public unsafe class CommandListDX12 : GpuCommandList
{
    bool _isClosed;
    ID3D12GraphicsCommandList* _handle;

    internal CommandListDX12(CommandAllocatorDX12 allocator, ID3D12GraphicsCommandList* handle) :
        base(allocator.Device)
    {
        Device = allocator.Device;
        Allocator = allocator;
        Fence = new FenceDX12(allocator.Device, FenceFlags.None);
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

    internal void Reset(CommandAllocatorDX12 allocator, PipelineStateDX12 initialState)
    {
        if(!_isClosed)
            throw new InvalidOperationException("Command list must be closed before it can be reset.");

        ID3D12PipelineState* pState = initialState != null ? initialState.Handle : null;
        Handle->Reset(allocator.Handle, pState);
        _isClosed = false;
    }

    protected override void CopyResource(GpuResource src, GpuResource dest)
    {
        src.Apply(this);
        dest.Apply(this);

        _handle->CopyResource((ResourceHandleDX12)dest.Handle, (ResourceHandleDX12)src.Handle);
        Profiler.ResourceCopyCalls++;
    }

    public void Close()
    {
        if (!_isClosed)
        {
            Handle->Close();
            _isClosed = true;
        }
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
            GraphicsBuffer iBuffer = State.IndexBuffer.BoundValue;
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
        PipelineStateDX12 state = _stateBuilder.Build(pass, _inputLayout);

        _handle->SetPipelineState(state.Handle);
        _handle->SetGraphicsRootSignature(state.RootSignature.Handle);

        Device.Heap.PrepareGpuHeap(pass, _cmd);

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

    public override void Wait(ulong nsTimeout = ulong.MaxValue)
    {
        Fence.Wait(nsTimeout);
    }

    public override void Free()
    {
        throw new NotImplementedException();
    }

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref _handle);
        Fence?.Dispose();

        Allocator.Unallocate(this);
    }

    /// <summary>
    /// Gets the parent <see cref="CommandAllocatorDX12"/> from which the current <see cref="CommandListDX12"/> was allocated.
    /// </summary>
    internal CommandAllocatorDX12 Allocator { get; }

    public CommandListType Type => Allocator.Type;

    internal FenceDX12 Fence { get; }

    public static implicit operator ID3D12CommandList*(CommandListDX12 cmd) => (ID3D12CommandList*)cmd._handle;

    public unsafe ID3D12CommandList* BaseHandle => (ID3D12CommandList*)_handle;

    internal ref readonly ID3D12GraphicsCommandList* Handle => ref _handle;

    public new DeviceDX12 Device { get; }
}
