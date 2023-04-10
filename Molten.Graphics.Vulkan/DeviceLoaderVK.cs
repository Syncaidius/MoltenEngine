using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal unsafe class DeviceLoaderVK : ExtensionLoaderVK<Device>
    {
        DeviceVK _device;
        Instance* _vkInstance;
        CommandSetCapabilityFlags _cap;
        DeviceQueueCreateInfo* _queueInfo;
        uint _queueCount = 0;

        public DeviceLoaderVK(RendererVK renderer, DeviceVK device, CommandSetCapabilityFlags requiredCap) : base(renderer)
        {
            _device = device;
            _vkInstance = renderer.Instance;
            _cap = requiredCap;
        }

        protected override unsafe Result GetLayers(uint* count, LayerProperties* items)
        {
            return Renderer.VK.EnumerateDeviceLayerProperties(_device.Adapter, count, items);
        }

        protected override unsafe Result GetExtensions(uint* count, ExtensionProperties* items)
        {
            byte* nullptr = null;
            return Renderer.VK.EnumerateDeviceExtensionProperties(_device.Adapter, nullptr, count, items);
        }

        protected override bool LoadExtension(RendererVK renderer, VulkanExtension ext, Device* obj)
        {
            return ext.Load(renderer, _vkInstance, obj);
        }

        protected override unsafe Result OnBuild(RendererVK renderer, VersionVK apiVersion, TempData tmp, ExtensionBinding binding, Device* obj)
        {
            List<SupportedCommandSet> sets = _device.Capabilities.CommandSets;
            if (_queueInfo != null)
                EngineUtil.Free(ref _queueInfo);

            _queueInfo = EngineUtil.AllocArray<DeviceQueueCreateInfo>((uint)sets.Count);

            CommandSetCapabilityFlags[] values = Enum.GetValues<CommandSetCapabilityFlags>();
            CommandSetCapabilityFlags capActive = CommandSetCapabilityFlags.None;

            _queueCount = 0;
            float queuePriority = 1.0f;

            // Iterate over all available command set/queue familes to find the requested capabilities (_cap).
            for (int i = 0; i < sets.Count; i++)
            {
                SupportedCommandSet set = sets[i];

                // Use the queue if it has at least one of the requested capabilities.
                foreach (CommandSetCapabilityFlags flag in values)
                {
                    if (flag == CommandSetCapabilityFlags.None)
                        continue;

                    if ((_cap & flag) == flag && (set.CapabilityFlags & flag) == flag)
                    {
                        _queueInfo[_queueCount++] = new DeviceQueueCreateInfo(StructureType.DeviceQueueCreateInfo)
                        {
                            Flags = DeviceQueueCreateFlags.None,
                            QueueCount = 1,
                            QueueFamilyIndex = (uint)i,
                            PQueuePriorities = &queuePriority
                        };

                        capActive |= set.CapabilityFlags;

                        break;
                    }
                }
            }

            Result r = Result.Success;

            // Check if all requested capabilities were available.
            if ((capActive & _cap) != _cap)
            {
                r = Result.ErrorInitializationFailed;
                Renderer.Log.Error($"Not all requested capabilities were supported. Requested: '{_cap}' -- Found: '{capActive}'");
            }
            else
            {
                DeviceCreateInfo createInfo = new DeviceCreateInfo()
                {
                    SType = StructureType.DeviceCreateInfo,
                    QueueCreateInfoCount = _queueCount,
                    PQueueCreateInfos = _queueInfo,
                    EnabledLayerCount = (uint)binding.Layers.Count,
                    PpEnabledLayerNames = tmp.LayerNames,
                    EnabledExtensionCount = (uint)binding.Extensions.Count,
                    PpEnabledExtensionNames = tmp.ExtensionNames
                };

                // Create the instance
                r = renderer.VK.CreateDevice(_device, &createInfo, null, obj);
                r.Check(renderer, () => $"Failed to create logical device on '{_device.Name}' adapter");
            }
            return r;
        }

        protected override void OnDispose()
        {
            EngineUtil.Free(ref _queueInfo);
            base.OnDispose();
        }

        internal DeviceQueueCreateInfo* QueueInfo => _queueInfo;

        internal uint QueueCount => _queueCount;
    }
}
