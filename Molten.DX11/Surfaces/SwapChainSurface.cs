using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Molten.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    /// <summary>A render target that is created from, and outputs to, a device's swap chain.</summary>
    public abstract class SwapChainSurface : RenderSurfaceBase, ISwapChainSurface
    {
        protected SwapChain _swapChain;
        protected SwapChainDescription _swapDesc;

        int _vsync;

        internal SwapChainSurface(GraphicsDeviceDX11 device, int mipCount, int sampleCount)
            : base(device, 1,
                  1, SharpDX.DXGI.Format.B8G8R8A8_UNorm, mipCount, 1, sampleCount, TextureFlags.NoShaderResource)
        { }

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

        protected void SetVsync(bool enabled)
        {
            _vsync = enabled ? 1 : 0;
        }

        public void Present()
        {
            ApplyChanges(Device);

            if(OnPresent())
                _swapChain?.Present(_vsync, PresentFlags.None);
        }

        protected virtual bool OnPresent() { return true; }

        public Color PresentClearColor { get; set; } = new Color(0, 0, 0, 255);
    }
}
