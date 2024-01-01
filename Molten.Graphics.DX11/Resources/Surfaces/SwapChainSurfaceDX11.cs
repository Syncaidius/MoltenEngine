using Molten.Collections;
using Molten.Graphics.Dxgi;
using Molten.Windows32;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11
{
    /// <summary>A render target that is created from, and outputs to, a device's swap chain.</summary>
    public unsafe abstract class SwapChainSurfaceDX11 : RenderSurface2DDX11, ISwapChainSurface
    {
        protected internal IDXGISwapChain4* NativeSwapChain;

        PresentParameters* _presentParams;
        SwapChainDesc1 _swapDesc;

        ThreadedQueue<Action> _dispatchQueue;
        uint _vsync;

        internal SwapChainSurfaceDX11(DeviceDX11 device, uint width, uint height, uint mipCount, GraphicsFormat format = GraphicsFormat.B8G8R8A8_UNorm)
            : base(device, width, height, 
                  GraphicsResourceFlags.NoShaderAccess | GraphicsResourceFlags.None | GraphicsResourceFlags.GpuWrite,
                  format, mipCount, 1, AntiAliasLevel.None, MSAAQuality.Default)
        {
            _dispatchQueue = new ThreadedQueue<Action>();
            _presentParams = EngineUtil.Alloc<PresentParameters>();
            _presentParams[0] = new PresentParameters();
        }

        protected override void CreateTexture(DeviceDX11 device, ResourceHandleDX11<ID3D11Resource> handle, uint handleIndex)
        {
            // Resize the swap chain if needed.
            if (NativeSwapChain != null)
            {
                FreeOldHandles(device.Renderer.FrameID);
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
            void* ppSurface = null;
            Guid riid = ID3D11Texture2D1.Guid;
            WinHResult hr = NativeSwapChain->GetBuffer(handleIndex, &riid, &ppSurface);
            DxgiError err = hr.ToEnum<DxgiError>();

            SurfaceHandleDX11 rsHandle = handle as SurfaceHandleDX11;
            rsHandle.RTV.Desc.Format = DxgiFormat;

            if (err == DxgiError.Ok)
            {
                rsHandle.RTV.Desc = new RenderTargetViewDesc1()
                {
                    Format = _swapDesc.Format,
                    ViewDimension = RtvDimension.Texture2D,
                };

                handle.NativePtr = (ID3D11Resource*)ppSurface;
                rsHandle.RTV.Create();
                Viewport = new ViewportF(0, 0, Width, Height);
            }
            else
            {
                Device.Log.Error($"Error creating resource for SwapChainSurface '{Name}': {err}");
            }
        }

        protected abstract void OnCreateSwapchain(ref Texture2DDesc1 desc);

        protected void CreateSwapChain(DisplayModeDXGI mode, bool windowed, IntPtr controlHandle)
        {
            GraphicsManagerDXGI dxgiManager = Device.Manager as GraphicsManagerDXGI;

            NativeSwapChain = dxgiManager.CreateSwapChain(mode, SwapEffect.FlipDiscard, Device.FrameBufferSize, Device.Log, (IUnknown*)Device.Ptr, controlHandle);
        }

        private void VSync_OnChanged(bool oldValue, bool newValue)
        {
            _vsync = newValue ? 1U : 0;
        }

        protected override uint GetMaxFrameBufferSize(uint frameBufferSize)
        {
            return 1;
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
}
