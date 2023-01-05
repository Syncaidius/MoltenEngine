using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Molten.Graphics.Hardware;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal unsafe class DeviceVK : ExtensionManager<Device>
    {
        Instance* _vkInstance;
        Logger _log;

        internal DeviceVK(RendererVK renderer, DisplayAdapterVK adapter, Instance* instance) : 
            base(renderer)
        {
            _log = renderer.Log;
            _vkInstance = instance;
            Adapter = adapter;
        }

        protected override bool LoadExtension(RendererVK renderer, VulkanExtension ext, Device* obj)
        {
            return ext.Load(renderer, _vkInstance, obj);
        }

        protected unsafe override void DestroyObject(RendererVK renderer, Device* obj)
        {
            renderer.VK.DestroyDevice(*obj, null);
        }

        protected override unsafe Result OnBuild(RendererVK renderer, VersionVK apiVersion, TempData tmp, ExtensionBinding binding, Device* obj)
        {
            // TODO build queue create info

            DeviceCreateInfo createInfo = new DeviceCreateInfo()
            {
                SType = StructureType.DeviceCreateInfo,
                QueueCreateInfoCount = 0,
                PQueueCreateInfos = null,
                EnabledLayerCount = (uint)binding.Layers.Count,
                PpEnabledLayerNames = tmp.LayerNames,
                EnabledExtensionCount = (uint)binding.Extensions.Count,
                PpEnabledExtensionNames = tmp.ExtensionNames
            };

            // Create the instance
            return renderer.VK.CreateDevice(Adapter, &createInfo, null, obj);
        }

        /// <summary>
        /// Gets the underlying <see cref="DisplayAdapterVK"/> that the current <see cref="DeviceVK"/> is bound to.
        /// </summary>
        internal DisplayAdapterVK Adapter { get; }
    }
}
