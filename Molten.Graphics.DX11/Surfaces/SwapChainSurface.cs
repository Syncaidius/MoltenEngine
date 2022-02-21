using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Molten.Collections;
using Molten.Graphics.Dxgi;
using Silk.NET.DXGI;
using Silk.NET.Direct3D11;
using Silk.NET.Core.Native;
using Molten.Windows32;

namespace Molten.Graphics
{
    /// <summary>A render target that is created from, and outputs to, a device's swap chain.</summary>
    public unsafe abstract class SwapChainSurface : RenderSurface, ISwapChainSurface
    {
        protected internal IDXGISwapChain1* NativeSwapChain;

        PresentParameters* _presentParams;
        SwapChainDesc1 _swapDesc;

        ThreadedQueue<Action> _dispatchQueue;
        uint _vsync;

        internal SwapChainSurface(RendererDX11 renderer, uint mipCount, uint sampleCount)
            : base(renderer, 1, 1, Format.FormatB8G8R8A8Unorm, mipCount, 1, sampleCount, TextureFlags.NoShaderResource)
        {
            _dispatchQueue = new ThreadedQueue<Action>();
            _presentParams = EngineUtil.Alloc<PresentParameters>();
        }

        protected void CreateSwapChain(DisplayMode mode, bool windowed, IntPtr controlHandle)
        {
            _swapDesc = new SwapChainDesc1()
            {
                Width = mode.Width,
                Height = mode.Height,
                Format = mode.Format,
                BufferUsage = (uint)DxgiUsage.RenderTargetOutput,
                BufferCount = Device.Settings.BackBufferSize,
                SampleDesc = new SampleDesc(1, 0),
                SwapEffect = SwapEffect.SwapEffectDiscard,
                Flags = (uint)DxgiSwapChainFlags.None,
                Stereo = 0,
                Scaling = Scaling.ScalingStretch,
                AlphaMode = AlphaMode.AlphaModeIgnore // TODO implement this correctly
            };

            WinHResult hr = Device.DisplayManager.DxgiFactory->CreateSwapChainForHwnd((IUnknown*)Device.NativeDevice, controlHandle, ref _swapDesc, null, null, ref NativeSwapChain);
            DxgiError de = hr.ToEnum<DxgiError>();

            if (de != DxgiError.Ok)
                Renderer.Log.WriteError($"Creation of swapchain failed with result: {de}");
        }

        protected override unsafe ID3D11Resource* CreateResource(bool resize)
        {
            // Resize the swap chain if needed.
            if (resize && NativeSwapChain != null)
            {
                NativeSwapChain->ResizeBuffers(_swapDesc.BufferCount, Width, Height, GraphicsFormat.Unknown.ToApi(), 0U);
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
            Guid riid = ID3D11Texture2D.Guid;
            NativeSwapChain->GetBuffer(0, &riid, &ppSurface);

            RenderTargetViewDesc rtvDesc = new RenderTargetViewDesc()
            {
                Format = _swapDesc.Format,
                ViewDimension = RtvDimension.RtvDimensionTexture2D,
            };

            ID3D11Resource* res = (ID3D11Resource*)NativeTexture;
            Device.NativeDevice->CreateRenderTargetView(res, &rtvDesc, ref RTV);
            VP = new ViewportF(0, 0, Width, Height);

            return res;
        }

        protected abstract void OnSwapChainMissing();

        private void VSync_OnChanged(bool oldValue, bool newValue)
        {
            _vsync = newValue ? 1U : 0;
        }

        public void Present()
        {
            Apply(Device);

            if (OnPresent() && NativeSwapChain != null)
            {
                
                NativeSwapChain->Present1(_vsync, 0U, _presentParams);
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
            base.PipelineDispose();
        }

        public void Dispatch(Action action)
        {
            _dispatchQueue.Enqueue(action);
        }

        internal override void PipelineDispose()
        {
            SilkUtil.ReleasePtr(ref NativeSwapChain);
            EngineUtil.Free(ref _presentParams);
            base.PipelineDispose();
        }

        protected abstract bool OnPresent();
    }
}
