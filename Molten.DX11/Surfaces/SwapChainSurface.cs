using Molten.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Molten.Collections;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    /// <summary>A render target that is created from, and outputs to, a device's swap chain.</summary>
    public abstract class SwapChainSurface : RenderSurface, ISwapChainSurface
    {
        protected SwapChain _swapChain;
        protected SwapChainDescription _swapDesc;

        ThreadedQueue<Action> _dispatchQueue;
        int _vsync;

        internal SwapChainSurface(RendererDX11 renderer, int mipCount, int sampleCount)
            : base(renderer, 1, 1, Format.FormatB8G8R8A8Unorm, mipCount, 1, sampleCount, TextureFlags.NoShaderResource)
        {
            _dispatchQueue = new ThreadedQueue<Action>();
        }

        protected void CreateSwapChain(DisplayMode mode, bool windowed, IntPtr controlHandle)
        {
            SwapChainDescription desc = new SwapChainDescription()
            {
                BufferCount = Device.Settings.BackBufferSize,
                ModeDescription = mode.Description,
                IsWindowed = true,
                OutputHandle = controlHandle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput,
                Flags = SwapChainFlags.None,
            };

            _swapChain = new SwapChain(Device.DisplayManager.DxgiFactory, Device.D3d, desc);
            _swapDesc = desc;
        }

        protected override SharpDX.Direct3D11.Resource CreateResource(bool resize)
        {
            // Resize the swap chain if needed.
            if (resize && _swapChain != null)
            {
                _swapChain.ResizeBuffers(_swapDesc.BufferCount, Width, Height, GraphicsFormat.Unknown.ToApi(), SwapChainFlags.None);
                _swapDesc = _swapChain.Description;
            }
            else
            {
                _swapChain?.Dispose();
                OnSwapChainMissing();

                _vsync = Device.Settings.VSync ? 1 : 0;
                Device.Settings.VSync.OnChanged += VSync_OnChanged;
            }

            // Create new backbuffer from swap chain.
            _texture = Texture2D.FromSwapChain<Texture2D>(_swapChain, 0);
            _rtv = new RenderTargetView(Device.D3d, _texture);
            VP = new Viewport(0, 0, (int)Width, (int)Height);

            return _texture;
        }


        protected abstract void OnSwapChainMissing();

        private void VSync_OnChanged(bool oldValue, bool newValue)
        {
            _vsync = newValue ? 1 : 0;
        }

        public void Present()
        {
            Apply(Device);

            if(OnPresent())
                _swapChain?.Present(_vsync, Present.None);

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
            base.OnPipelineDispose();
        }

        public void Dispatch(Action action)
        {
            _dispatchQueue.Enqueue(action);
        }

        protected virtual bool OnPresent() { return true; }
    }
}
