using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silk.NET.Vulkan;

namespace Molten.Graphics
{
    internal unsafe class DeviceVK : ExtensionManager<Device>
    {
        Instance* _vkInstance;
        RendererVK _renderer;
        CommandSetCapabilityFlags _cap;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="adapter"></param>
        /// <param name="instance"></param>
        /// <param name="requiredCap">Required capabilities</param>
        internal DeviceVK(RendererVK renderer, DisplayAdapterVK adapter, Instance* instance, CommandSetCapabilityFlags requiredCap) : 
            base(renderer)
        {
            _renderer = renderer;
            _vkInstance = instance;
            Adapter = adapter;
            _cap = requiredCap;
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
            List<SupportedCommandSet> sets = Adapter.Capabilities.CommandSets;
            DeviceQueueCreateInfo* queueInfo = EngineUtil.AllocArray<DeviceQueueCreateInfo>((uint)sets.Count);

            CommandSetCapabilityFlags[] values = Enum.GetValues<CommandSetCapabilityFlags>();
            CommandSetCapabilityFlags capActive = CommandSetCapabilityFlags.None;

            uint queueCount = 0;
            float queuePriority = 1.0f;

            // Iterate over all available command set/queue familes to find the requested capabilities (_cap).
            for (int i = 0; i < sets.Count; i++)
            {
                SupportedCommandSet set = sets[i];

                // Use the queue if it has at least one of the requested capabilities.
                foreach(CommandSetCapabilityFlags flag in values)
                {
                    if (flag == CommandSetCapabilityFlags.None)
                        continue;

                    if((_cap & flag) == flag && (set.CapabilityFlags & flag) == flag)
                    {
                        queueInfo[queueCount++] = new DeviceQueueCreateInfo(StructureType.DeviceQueueCreateInfo)
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
                _renderer.Log.Error($"Not all requested capabilities were supported. Requested: '{_cap}' -- Found: '{capActive}'");
            }
            else
            {

                DeviceCreateInfo createInfo = new DeviceCreateInfo()
                {
                    SType = StructureType.DeviceCreateInfo,
                    QueueCreateInfoCount = queueCount,
                    PQueueCreateInfos = queueInfo,
                    EnabledLayerCount = (uint)binding.Layers.Count,
                    PpEnabledLayerNames = tmp.LayerNames,
                    EnabledExtensionCount = (uint)binding.Extensions.Count,
                    PpEnabledExtensionNames = tmp.ExtensionNames
                };

                // Create the instance
                r = renderer.VK.CreateDevice(Adapter, &createInfo, null, obj);
            }

            EngineUtil.Free(ref queueInfo);
            return r;
        }

        /// <summary>
        /// Gets the underlying <see cref="DisplayAdapterVK"/> that the current <see cref="DeviceVK"/> is bound to.
        /// </summary>
        internal DisplayAdapterVK Adapter { get; }

        /// <summary>
        /// Gets the <see cref="Instance"/> that the current <see cref="DeviceVK"/> is bound to.
        /// </summary>
        internal Instance* Instance => _vkInstance;
    }
}
