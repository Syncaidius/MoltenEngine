using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal unsafe class InstanceLoaderVK : ExtensionLoaderVK<Instance>
{
    internal InstanceLoaderVK(RendererVK renderer) : base(renderer) { }

    protected override unsafe Result GetLayers(uint* count, LayerProperties* items)
    {
        return Renderer.VK.EnumerateInstanceLayerProperties(count, items);
    }

    protected override unsafe Result GetExtensions(uint* count, ExtensionProperties* items)
    {
        byte * nullptr = null;
        return Renderer.VK.EnumerateInstanceExtensionProperties(nullptr, count, items);
    }

    protected override bool LoadExtensionModule(RendererVK renderer, VulkanExtension ext, Instance* obj)
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
            AddExtension(name);
        }
    }

    protected override unsafe Result OnBuild(RendererVK renderer, VersionVK apiVersion, TempData tmp, ExtensionBinding binding, Instance* obj)
    {
        ApplicationInfo appInfo = new ApplicationInfo()
        {
            SType = StructureType.ApplicationInfo,
            PEngineName = (byte*)SilkMarshal.StringToPtr("Molten Engine", NativeStringEncoding.UTF8),
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
        Result r = renderer.VK.CreateInstance(&createInfo, null, obj);
        SilkMarshal.FreeString((nint)appInfo.PEngineName);

        return r;
    }
}
