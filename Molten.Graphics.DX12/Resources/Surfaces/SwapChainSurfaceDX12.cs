using Molten.Collections;
using Molten.Graphics.Dxgi;
using Molten.Windows32;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX12;

/// <summary>A render target that is created from, and outputs to, a device's swap chain.</summary>
public unsafe abstract class SwapChainSurfaceDX12 : RenderSurface2DDX12, ISwapChainSurface
{
    protected internal IDXGISwapChain4* NativeSwapChain;

    PresentParameters* _presentParams;
    SwapChainDesc1 _swapDesc;
    ID3D12Resource1* _surfaces;
    SwapChainHandleDX12 _handle;
    ThreadedQueue<Action> _dispatchQueue;

    uint _vsync;

    internal SwapChainSurfaceDX12(DeviceDX12 device, uint width, uint height, uint mipCount, GraphicsFormat format = GraphicsFormat.B8G8R8A8_UNorm)
        : base(device, width, height, 
              GraphicsResourceFlags.DenyShaderAccess | GraphicsResourceFlags.None | GraphicsResourceFlags.GpuWrite,
              format, mipCount, 1, AntiAliasLevel.None, MSAAQuality.Default)
    {
        _dispatchQueue = new ThreadedQueue<Action>();
        _presentParams = EngineUtil.Alloc<PresentParameters>();
        _presentParams[0] = new PresentParameters();
    }

    protected override void OnCreateResource()
    {
        //base.OnCreateResource();
    }

    protected override unsafe ResourceHandleDX12 OnCreateHandle(ID3D12Resource1* ptr)
    {
        // Resize the swap chain if needed.
        if (NativeSwapChain != null)
        {
            WinHResult result = NativeSwapChain->ResizeBuffers(Device.FrameBufferSize, Width, Height, GraphicsFormat.Unknown.ToApi(), 0U);
            NativeSwapChain->GetDesc1(ref _swapDesc);
        }
        else
        {
            NativeUtil.ReleasePtr(ref NativeSwapChain);
            OnCreateSwapchain(ref Desc);
            NativeSwapChain->GetDesc1(ref _swapDesc);

            _vsync = Device.Settings.VSync ? 1U : 0;
            Device.Settings.VSync.OnChanged += VSync_OnChanged;
        }

        /* NOTE:
         *  Discard Mode = Only image index 0 can be accessed
         *  Sequential/FlipS-Sequential Modes = Only image index 0 can be accesed for writing. The rest can only be accesed for reading.
         *  
         *  This means we only need 1 handle for the swap chain, as the next image is always at index 0.
         */
        ID3D12Resource1** ptrHandles = stackalloc ID3D12Resource1*[(int)Device.FrameBufferSize];

        for (uint i = 0; i < Device.FrameBufferSize; i++)
        {
            void* ppSurface = null;
            Guid riid = ID3D12Resource1.Guid;
            WinHResult hr = NativeSwapChain->GetBuffer(i, &riid, &ppSurface);
            DxgiError err = hr.ToEnum<DxgiError>();
            ptrHandles[i] = (ID3D12Resource1*)ppSurface;

            if (err != DxgiError.Ok)
            {
                Device.Log.Error($"Error retrieving resource for SwapChainSurface '{Name}' frame index {i}: {err}");
                return null;
            }
        }

        SwapChainHandleDX12 handle = new SwapChainHandleDX12(this, ptrHandles, Device.FrameBufferSize);
        handle.View.Desc = new RenderTargetViewDesc()
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

        return handle;
    }

    protected virtual void SetRTVDescription(ref RenderTargetViewDesc desc) { }

    protected abstract void OnCreateSwapchain(ref ResourceDesc1 desc);

    protected void CreateSwapChain(DisplayModeDXGI mode, IntPtr controlHandle)
    {
        GraphicsManagerDXGI dxgiManager = Device.Manager as GraphicsManagerDXGI;

        NativeSwapChain = dxgiManager.CreateSwapChain(mode, SwapEffect.FlipDiscard, Device.FrameBufferSize, Device.Log, (IUnknown*)Device.Ptr, controlHandle);
    }

    private void VSync_OnChanged(bool oldValue, bool newValue)
    {
        _vsync = newValue ? 1U : 0;
    }

    DxgiError _lastError;
    internal void Present()
    {
        Apply(Device.Queue);

        if (OnPresent() && NativeSwapChain != null)
        {
            // TODO implement partial-present - Partial Presentation (using scroll or dirty rects)
            // is not valid until first submitting a regular Present without scroll or dirty rects.
            // Otherwise, the preserved back-buffer data would be uninitialized.

            // See for flags: https://learn.microsoft.com/en-us/windows/win32/direct3ddxgi/dxgi-present
            WinHResult r = NativeSwapChain->Present1(_vsync, 0U, _presentParams);
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
        NativeUtil.ReleasePtr(ref NativeSwapChain);
        EngineUtil.Free(ref _presentParams);
        base.OnGraphicsRelease();
    }

    protected abstract bool OnPresent();

    /// <inheritdoc/>
    public bool IsEnabled { get; set; }
}
