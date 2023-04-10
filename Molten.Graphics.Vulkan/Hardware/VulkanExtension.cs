using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal abstract class VulkanExtension
    {
        internal unsafe abstract bool Load(RendererVK renderer, Instance* instance, Device* device);

        public unsafe abstract void Unload(RendererVK renderer);
    }

    internal class VulkanBasicExtension : VulkanExtension
    {
        public override void Unload(RendererVK renderer) { }

        internal override unsafe bool Load(RendererVK renderer, Instance* instance, Device* device) { return true; }
    }

    internal class VulkanExtension<E> : VulkanExtension
        where E : NativeExtension<Vk>
    {
        internal E Module { get; set; }

        protected Action<E> LoadCallback;
        protected Action<E> UnloadCallback;

        internal VulkanExtension(Action<E> loadCallback, Action<E> unloadCallback)
        {
            LoadCallback = loadCallback;
            UnloadCallback = unloadCallback;
        }

        internal unsafe override sealed bool Load(RendererVK renderer, Instance* instance, Device* device)
        {
            bool success = false;
            E ext;

            if (device == null)
                success = renderer.VK.TryGetInstanceExtension(*instance, out ext);
            else
                success = renderer.VK.TryGetDeviceExtension(*instance, *device, out ext);

            if (success)
            {
                Module = ext;
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
            if (Module != null)
            {
                Module.Dispose();
                UnloadCallback?.Invoke(Module);
                Module = null;
            }
        }
    }
}
