using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal unsafe class WindowSurfaceVK : NativeObjectVK<SurfaceKHR>
    {
        CommandQueueVK _presentQueue;
        WindowHandle* _window;

        internal unsafe WindowSurfaceVK(RendererVK renderer, string title, uint width, uint height)
        {
            Renderer = renderer;
            renderer.GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);
            renderer.GLFW.WindowHint(WindowHintBool.Resizable, true);
            _window = renderer.GLFW.CreateWindow((int)width, (int)height, title, null, null);

            VkHandle instanceHandle = new VkHandle(renderer.Instance.Ptr->Handle);
            VkNonDispatchableHandle* surfaceHandle = null;
            Result r = (Result)renderer.GLFW.CreateWindowSurface(instanceHandle, _window, null, surfaceHandle);
            if (!renderer.CheckResult(r))
                return;

            Native = new SurfaceKHR(surfaceHandle->Handle);
            _presentQueue = renderer.Device.FindPresentQueue(this);

            if (_presentQueue == null)
                renderer.Log.Error($"No command queue found to present window surface");
        }

        protected override void OnDispose()
        {
            if (_window != null)
                Renderer.GLFW.DestroyWindow(_window);
        }

        /// <summary>
        /// Gets the <see cref="RendererVK"/> instance that the current <see cref="WindowSurfaceVK"/> is bound to.
        /// </summary>
        public RendererVK Renderer { get; }
    }
}
