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

        internal SwapChainSurfaceDX11(GraphicsDevice device, uint mipCount, GraphicsFormat format = GraphicsFormat.B8G8R8A8_UNorm)
            : base(device, 1, 1, 
                  GraphicsResourceFlags.NoShaderAccess | GraphicsResourceFlags.None | GraphicsResourceFlags.GpuWrite,
                  format, mipCount, 1, AntiAliasLevel.None, MSAAQuality.Default)
        {
            _dispatchQueue = new ThreadedQueue<Action>();
            _presentParams = EngineUtil.Alloc<PresentParameters>();
        }

        protected void CreateSwapChain(DisplayModeDXGI mode, bool windowed, IntPtr controlHandle)
        {
            DeviceDX11 nativeDevice = (Device as DeviceDX11);
            NativeSwapChain = (Device.Manager as GraphicsManagerDXGI).CreateSwapChain(mode, Device.Settings, Device.Log, (IUnknown*)nativeDevice.Ptr, controlHandle);
        }

        protected override void CreateTexture(DeviceDX11 device, ResourceHandleDX11<ID3D11Resource> handle, uint handleIndex)
        {
            RTV.Release();

            // Resize the swap chain if needed.
            if (resize && NativeSwapChain != null)
            {
                NativeSwapChain->ResizeBuffers(Device.Settings.GetBackBufferSize(), Width, Height, GraphicsFormat.Unknown.ToApi(), 0U);
                NativeSwapChain->GetDesc1(ref _swapDesc);
            }
            else
            {
                SilkUtil.ReleasePtr(ref NativeSwapChain);
                OnSwapChainMissing();

                _vsync = Device.Settings.VSync ? 1U : 0;
                Device.Settings.VSync.OnChanged += VSync_OnChanged;
            }

            void* ppSurface = null;
            ID3D11Resource* res = null;
            Guid riid = ID3D11Texture2D1.Guid;
            WinHResult hr = NativeSwapChain->GetBuffer(handleIndex, &riid, &ppSurface);
            DxgiError err = hr.ToEnum<DxgiError>();
            if (err == DxgiError.Ok)
            {
                RTV.Desc = new RenderTargetViewDesc1()
                {
                    Format = _swapDesc.Format,
                    ViewDimension = RtvDimension.Texture2D,
                };

                res = (ID3D11Resource*)ppSurface;
                RTV.Create(res);
                Viewport = new ViewportF(0, 0, Width, Height);
            }
            else
            {
                Device.Log.Error($"Error creating resource for SwapChainSurface '{Name}': {err}");
            }
        }

        protected abstract void OnSwapChainMissing();

        private void VSync_OnChanged(bool oldValue, bool newValue)
        {
            _vsync = newValue ? 1U : 0;
        }

        internal void Present()
        {
            OnApply(Device.Queue);

            if (OnPresent() && NativeSwapChain != null)
            {
                NativeSwapChain->Present(_vsync, 0U);

                // TODO implement partial-present - Partial Presentation (using scroll or dirty rects)
                // is not valid until first submitting a regular Present without scroll or dirty rects.
                // Otherwise, the preserved back-buffer data would be uninitialized.

                // NativeSwapChain->Present1(_vsync, 0U, _presentParams);
            }

            if (!IsDisposed)
            {
                while (_dispatchQueue.TryDequeue(out Action action))
                    action();
            }
        }

        protected override void DisposeTextureForResize()
        {
            // Skip calling the SwapChainSurfaceDX11.OnGraphicsDispose() implementation. Jump straight to base.
            // This prevents swapchain render loops from being aborted due to disposal flags being set.
            base.OnGraphicsRelease();
        }

        public void Dispatch(Action action)
        {
            _dispatchQueue.Enqueue(action);
        }

        protected override void OnGraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref NativeSwapChain);
            EngineUtil.Free(ref _presentParams);
            base.OnGraphicsRelease();
        }

        protected abstract bool OnPresent();

        /// <inheritdoc/>
        public bool IsEnabled { get; set; }
    }
}
