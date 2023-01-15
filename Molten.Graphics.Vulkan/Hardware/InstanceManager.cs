using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal unsafe class InstanceManager : ExtensionManager<Instance>
    {
        internal InstanceManager(RendererVK renderer) : base(renderer) { }

        protected override LayerProperties[] GetLayers(string typeName)
        {
            return Renderer.Enumerate<LayerProperties>(Renderer.VK.EnumerateInstanceLayerProperties, $"{typeName} layers");
        }

        protected override ExtensionProperties[] GetExtensions(string typeName)
        {
            return Renderer.Enumerate<ExtensionProperties>((count, items) =>
            {
                byte* nullptr = null;
                return Renderer.VK.EnumerateInstanceExtensionProperties(nullptr, count, items);
            }, $"{typeName} extensions");
        }

        protected override bool LoadExtension(RendererVK renderer, VulkanExtension ext, Instance* obj)
        {
            return ext.Load(renderer, obj, null);
        }

        internal void AddGlfwExtensions()
        {
            uint glfwCount = 0;

            byte** glfwNames = Renderer.GLFW.GetRequiredInstanceExtensions(out glfwCount);
            for(uint i = 0; i < glfwCount; i++)
            {
                string name = SilkMarshal.PtrToString((nint)glfwNames[i], NativeStringEncoding.UTF8);
                AddBasicExtension(name);
            }
        }

        protected unsafe override void DestroyObject(RendererVK renderer, Instance* obj)
        {
            renderer.VK.DestroyInstance(*obj, null);
        }

        protected override unsafe Result OnBuild(RendererVK renderer, VersionVK apiVersion, TempData tmp, ExtensionBinding binding, Instance* obj)
        {
            ApplicationInfo appInfo = new ApplicationInfo()
            {
                SType = StructureType.ApplicationInfo,
                EngineVersion = 1,
                ApiVersion = apiVersion,
            };

            InstanceCreateInfo createInfo = new InstanceCreateInfo()
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
                EnabledLayerCount = (uint)binding.Layers.Count,
                PpEnabledLayerNames = tmp.LayerNames,
                EnabledExtensionCount = (uint)binding.Extensions.Count,
                PpEnabledExtensionNames = tmp.ExtensionNames
            };

            // Create the instance
            return renderer.VK.CreateInstance(&createInfo, null, obj);
        }
    }
}
