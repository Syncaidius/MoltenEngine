using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal class DeviceManager : ExtensionManager<Device>
    {
        Instance _vkInstance;
        PhysicalDevice _pDevice;

        internal DeviceManager(RendererVK renderer, PhysicalDevice pDevice, Instance instance) : base(renderer)
        {
            _vkInstance = instance;
            _pDevice = pDevice;
        }

        protected override nint GetObjectHandle(Device obj)
        {
            return obj.Handle;
        }

        protected override bool LoadExtension(RendererVK renderer, VulkanExtension ext, Device obj)
        {
            return ext.Load(renderer, _vkInstance, obj);
        }

        protected unsafe override void DestroyObject(RendererVK renderer, Device obj)
        {
            renderer.VK.DestroyDevice(obj, null);
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
            return renderer.VK.CreateDevice(_pDevice, &createInfo, null, obj);
        }
    }
}
