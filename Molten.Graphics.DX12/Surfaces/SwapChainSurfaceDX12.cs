using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics.Dxgi;
using Molten.Windows32;
using Silk.NET.Core.Native;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public unsafe class SwapChainSurfaceDX12
    {
        protected internal IDXGISwapChain4* NativeSwapChain;

        internal SwapChainSurfaceDX12(RendererDX12 renderer, uint mipCount)
        {
            Device = renderer.Device;
        }

        protected void CreateSwapChain(DisplayModeDXGI mode, bool windowed, IntPtr controlHandle)
        {
            NativeSwapChain = Device.DisplayManager.CreateSwapChain(mode, Device.Settings, Device.Log, (IUnknown*)Device.Ptr, controlHandle);
        }

        internal DeviceDX12 Device { get; }
    }
}
