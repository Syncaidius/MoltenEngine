using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal abstract class VulkanExtension
    {
        internal unsafe abstract bool Load(RendererVK renderer, Instance instance, Device device);

        public unsafe abstract void Unload(RendererVK renderer);
    }

    internal class VulkanExtension<E> : VulkanExtension
        where E : NativeExtension<Vk>
    {
        internal E Ext { get; set; }

        protected Action<E> LoadCallback;
        protected Action<E> UnloadCallback;

        internal VulkanExtension(Action<E> loadCallback, Action<E> unloadCallback)
        {
            LoadCallback = loadCallback;
            UnloadCallback = unloadCallback;
        }

        internal unsafe override sealed bool Load(RendererVK renderer, Instance instance, Device device)
        {
            bool success = false;
            E ext;

            if (device.Handle == IntPtr.Zero)
                success = renderer.VK.TryGetInstanceExtension(instance, out ext);
            else
                success = renderer.VK.TryGetDeviceExtension(instance, device, out ext);

            if (success)
            {
                Ext = ext;
                LoadCallback?.Invoke(ext);
            }
            else
            {
                renderer.Log.Error($"Failed to retrieve instance extension: '{typeof(E).Name}'");
            }

            return success;
        }

        public unsafe override void Unload(RendererVK renderer)
        {
            if (Ext != null)
            {
                Ext.Dispose();
                UnloadCallback?.Invoke(Ext);
                Ext = null;
            }
        }
    }
}
