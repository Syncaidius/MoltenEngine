using Molten.Collections;
using Molten.Graphics.Dxgi;
using Molten.Windows32;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
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

        protected override unsafe ID3D11Resource* CreateResource(bool resize)
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
            WinHResult hr = NativeSwapChain->GetBuffer(0, &riid, &ppSurface);
            DxgiError err = hr.ToEnum<DxgiError>();
            if (err == DxgiError.Ok)
            {
                NativeTexture = (ID3D11Texture2D1*)ppSurface;

                RTV.Desc = new RenderTargetViewDesc1()
                {
                    Format = _swapDesc.Format,
                    ViewDimension = RtvDimension.Texture2D,
                };

                res = (ID3D11Resource*)NativeTexture;
                RTV.Create(res);
                Viewport = new ViewportF(0, 0, Width, Height);
            }
            else
            {
                Device.Log.Error($"Error creating resource for SwapChainSurface '{Name}': {err}");
            }

            return res;
        }

        protected abstract void OnSwapChainMissing();

        private void VSync_OnChanged(bool oldValue, bool newValue)
        {
            _vsync = newValue ? 1U : 0;
        }

        public void Present()
        {
            OnApply(Device.Cmd);

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

        protected override void OnDisposeForRecreation()
        {
            // Avoid calling RenderFormSurface's OnPipelineDispose implementation by skipping it. Jump straight to base.
            // This prevents any swapchain render loops from being aborted due to disposal flags being set.
            base.GraphicsRelease();
        }

        public void Dispatch(Action action)
        {
            _dispatchQueue.Enqueue(action);
        }

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref NativeSwapChain);
            EngineUtil.Free(ref _presentParams);
            base.GraphicsRelease();
        }

        protected abstract bool OnPresent();
    }
}
