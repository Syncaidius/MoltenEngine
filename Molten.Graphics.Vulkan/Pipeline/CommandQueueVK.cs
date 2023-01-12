using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Molten.Graphics
{
    internal class CommandQueueVK : NativeObjectVK<Queue>
    {
        DeviceVK _device;

        internal CommandQueueVK(RendererVK renderer, DeviceVK device, uint familyIndex, Queue queue, uint queueIndex, SupportedCommandSet set)
        {
            VK = renderer.VK;
            Log = renderer.Log;
            Flags = set.CapabilityFlags;
            _device = device;
            Index = queueIndex;
            Native = queue;
            Set = set;
        }

        protected override void OnDispose()
        {
            
        }

        internal Vk VK { get; }

        internal Logger Log { get; }

        internal DeviceVK Device => _device;

        /// <summary>
        /// Gets the Queue family index, in relation to the bound <see cref="DeviceVK"/>.
        /// </summary>
        internal uint FamilyIndex { get; }

        /// <summary>
        /// Gets the command queue index, within its family.
        /// </summary>
        internal uint Index { get; }

        /// <summary>
        /// Gets the underlying command set definition.
        /// </summary>
        internal SupportedCommandSet Set { get; }

        internal CommandSetCapabilityFlags Flags { get; }
    }
}
