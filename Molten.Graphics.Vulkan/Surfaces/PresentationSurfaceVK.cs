using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Molten.Graphics
{
    internal class PresentationSurfaceVK : NativeObjectVK<SurfaceKHR>
    {
        CommandQueueVK _presentQueue;

        internal unsafe PresentationSurfaceVK(RendererVK renderer, WindowHandle* window)
        {

            VkHandle instanceHandle = new VkHandle(renderer.Instance.Ptr->Handle);
            VkNonDispatchableHandle* surfaceHandle = null;
            Result r = (Result)renderer.GLFW.CreateWindowSurface(instanceHandle, window, null, surfaceHandle);
            if (!renderer.CheckResult(r))
                return;

            Native = new SurfaceKHR(surfaceHandle->Handle);
            _presentQueue = renderer.Device.FindPresentQueue(this);

            if (_presentQueue == null)
                renderer.Log.Error($"No command queue found to present window surface");
        }

        protected override void OnDispose()
        {
            
        }
    }
}
