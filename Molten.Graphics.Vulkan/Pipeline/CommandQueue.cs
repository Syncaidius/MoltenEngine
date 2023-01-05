using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Pipeline
{
    internal abstract class CommandQueue
    {
        DeviceVK _device;

        internal CommandQueue(RendererVK renderer, DeviceVK device, CommandSetCapabilityFlags flags)
        {
            VK = renderer.VK;
            Log = renderer.Log;
            Flags = flags;
            _device = device;
        }

        internal Vk VK { get; }

        internal Logger Log { get; }

        internal DeviceVK Device => _device;

        internal CommandSetCapabilityFlags Flags { get; }
    }
}
