using Molten.Collections;
using Molten.Graphics.Dxgi;
using Molten.Windows32;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using System.Diagnostics;

namespace Molten.Graphics.DX12;

/// <summary>A render target that is created from, and outputs to, a device's swap chain.</summary>
public unsafe abstract class SwapChainSurfaceDX12 : RenderSurface2DDX12, ISwapChainSurface
{
    protected internal IDXGISwapChain4* SwapChainHandle;

    PresentParameters* _presentParams;
    SwapChainDesc1 _swapDesc;
    ID3D12Resource1** _ptrSurfaces;
    ThreadedQueue<Action> _dispatchQueue;

    uint _vsync;
    RTHandleDX12 _handle;

    internal SwapChainSurfaceDX12(DeviceDX12 device, uint width, uint height, uint mipCount, GpuResourceFormat format = GpuResourceFormat.R8G8B8A8_UNorm, string name = null)
        : base(device, width, height, 
              GpuResourceFlags.DenyShaderAccess | GpuResourceFlags.None | GpuResourceFlags.GpuWrite,
              format, mipCount, 1, AntiAliasLevel.None, MSAAQuality.Default, name)
    {
        _dispatchQueue = new ThreadedQueue<Action>();
        _presentParams = EngineUtil.Alloc<PresentParameters>();
        _presentParams[0] = new PresentParameters();
    }

    protected override unsafe ID3D12Resource1* OnCreateTexture()
    {
        // Resize the swap chain if needed.
        if (SwapChainHandle != null)
        {
            WinHResult result = SwapChainHandle->ResizeBuffers(Device.FrameBufferSize, Width, Height, GpuResourceFormat.Unknown.ToApi(), 0U);
            SwapChainHandle->GetDesc1(ref _swapDesc);
        }
        else
        {
            NativeUtil.ReleasePtr(ref SwapChainHandle);
            OnCreateSwapchain(ref Desc);

            if(SwapChainHandle != null)
                SwapChainHandle->GetDesc1(ref _swapDesc);
            else if(Debugger.IsAttached)
                throw new InvalidOperationException("Swap chain creation failed.");
            else
                Device.Log.Error("Swap chain creation failed.");

            _vsync = Device.Settings.VSync ? 1U : 0;
            Device.Settings.VSync.OnChanged += VSync_OnChanged;
        }

        /* NOTE:
         *  Discard Mode = Only image index 0 can be accessed
         *  Sequential/FlipS-Sequential Modes = Only image index 0 can be accesed for writing. The rest can only be accesed for reading.
         *  
         *  This means we only need 1 handle for the swap chain, as the next image is always at index 0.
         */
        _ptrSurfaces = EngineUtil.AllocPtrArray<ID3D12Resource1>(Device.FrameBufferSize);

        for (uint i = 0; i < Device.FrameBufferSize; i++)
        {
            void* ppSurface = null;
            Guid riid = ID3D12Resource1.Guid;
            WinHResult hr = SwapChainHandle->GetBuffer(i, &riid, &ppSurface);
            DxgiError err = hr.ToEnum<DxgiError>();
            _ptrSurfaces[i] = (ID3D12Resource1*)ppSurface;

            if (err != DxgiError.Ok)
            {
                Device.Log.Error($"Error retrieving resource for SwapChainSurface '{Name}' frame index {i}: {err}");
                return null;
            }
        }

        return _ptrSurfaces[0];
    }

    protected override unsafe ResourceHandleDX12 OnCreateHandle(ID3D12Resource1* ptr)
    {
        if(_handle == null)
            _handle = new RTHandleDX12(this, _ptrSurfaces, Device.FrameBufferSize);

        RenderTargetViewDesc desc = new()
        {
            Format = Desc.Format,
            ViewDimension = RtvDimension.Texture2Darray,
            Texture2DArray = new Tex2DArrayRtv()
            {
                ArraySize = Desc.DepthOrArraySize,
                MipSlice = 0,
                FirstArraySlice = 0,
                PlaneSlice = 0,
            },
        };

        _handle.RTV.Initialize(ref desc); 

        return _handle;
    }

    protected virtual void SetRTVDescription(ref RenderTargetViewDesc desc) { }

    protected abstract void OnCreateSwapchain(ref ResourceDesc1 desc);

    protected void CreateSwapChain(DisplayModeDXGI mode, IntPtr controlHandle)
    {
        // Swap-chain needs a D3D12 command queue so that it can force a swap-chain flush.
        IUnknown* cmdQueueHandle = (IUnknown*)Device.Queue.Handle;
        GraphicsManagerDXGI dxgiManager = Device.Manager as GraphicsManagerDXGI;

        DxgiError result = dxgiManager.CreateSwapChain(mode, SwapEffect.FlipDiscard, Device.FrameBufferSize, Device.Log, cmdQueueHandle, controlHandle, out SwapChainHandle);
        if (result == DxgiError.DeviceRemoved)
        {
            WinHResult hr = Device.Handle->GetDeviceRemovedReason();
            DxgiError dxgiReason = hr.ToEnum<DxgiError>();
            Device.Log.Error($"Device removed reason: {dxgiReason}");
        }
    }

    private void VSync_OnChanged(bool oldValue, bool newValue)
    {
        _vsync = newValue ? 1U : 0;
    }

    DxgiError _lastError;
    internal void Present()
    {
        Apply(Device.Queue);

        if (OnPresent() && SwapChainHandle != null)
        {
            // Update the RTV frame index, so that it points to the correct resource, SRV, UAV and RTV views.
            uint bbIndex = SwapChainHandle->GetCurrentBackBufferIndex();
            _handle.Index = bbIndex;

            // TODO implement partial-present - Partial Presentation (using scroll or dirty rects)
            // is not valid until first submitting a regular Present without scroll or dirty rects.
            // Otherwise, the preserved back-buffer data would be uninitialized.

            // See for flags: https://learn.microsoft.com/en-us/windows/win32/direct3ddxgi/dxgi-present
            WinHResult r = SwapChainHandle->Present1(_vsync, 0U, _presentParams);
            DxgiError de = r.ToEnum<DxgiError>();

            if (de != DxgiError.Ok)
            {
                if (_lastError != de)
                {
                    Device.Log.Error($"Creation of swapchain failed with result: {de}");
                    _lastError = de;
                }
            }
        }

        if (!IsDisposed)
        {
            while (_dispatchQueue.TryDequeue(out Action action))
                action();
        }
    }

    public void Dispatch(Action action)
    {
        _dispatchQueue.Enqueue(action);
    }

    protected override void OnGraphicsRelease()
    {
        NativeUtil.ReleasePtr(ref SwapChainHandle);
        EngineUtil.Free(ref _presentParams);
        base.OnGraphicsRelease();
    }

    protected abstract bool OnPresent();

    /// <inheritdoc/>
    public bool IsEnabled { get; set; }
}
