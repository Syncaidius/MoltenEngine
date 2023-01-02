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
        internal ExtensionProperties Properties;

        internal unsafe abstract void Load(RendererVK renderer, Instance instance);

        public unsafe abstract void Unload(RendererVK renderer, Instance instance);
    }

    internal class VulkanExtension<E> : VulkanExtension
        where E : NativeExtension<Vk>
    {
        internal E Ext { get; private set; }

        Action<E> _loadCallback;
        Action<E> _unloadCallback;

        internal VulkanExtension(Action<E> loadCallback, Action<E> unloadCallback)
        {
            _loadCallback = loadCallback;
            _unloadCallback = unloadCallback;
        }

        internal unsafe override void Load(RendererVK renderer, Instance instance)
        {
            if(renderer.VK.TryGetInstanceExtension(instance, out E ext))
            {
                Ext = ext;
                _loadCallback?.Invoke(ext);
            }
            else
            {
                renderer.Log.Error($"Failed to retrieve instance extension: '{typeof(E).Name}'");
            }
        }

        public unsafe override void Unload(RendererVK renderer, Instance instance)
        {
            if(Ext != null)
            {
                _unloadCallback?.Invoke(Ext);
                Ext = null;
            }
        }
    }
}
