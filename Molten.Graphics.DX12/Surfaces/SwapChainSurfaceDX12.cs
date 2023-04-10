using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX12
{
    public unsafe class SwapChainSurfaceDX12
    {
        protected internal IDXGISwapChain4* NativeSwapChain;

        internal SwapChainSurfaceDX12(RendererDX12 renderer, uint mipCount)
        {
            Device = renderer.NativeDevice;
        }

        protected void CreateSwapChain(DisplayModeDXGI mode, bool windowed, IntPtr controlHandle)
        {
            NativeSwapChain = (Device.Manager as GraphicsManagerDXGI).CreateSwapChain(mode, Device.Settings, Device.Log, (IUnknown*)Device.Ptr, controlHandle);
        }

        internal DeviceDX12 Device { get; }
    }
}
