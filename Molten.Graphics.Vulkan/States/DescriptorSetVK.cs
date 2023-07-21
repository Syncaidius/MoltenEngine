using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal class DescriptorSetVK : GraphicsObject
    {
        DescriptorSet _set;

        internal DescriptorSetVK(ShaderPassVK pass, DescriptorPoolVK pool, ref DescriptorSet set) : 
            base(pass.Device)
        {
            Pool = pool;
            _set = set;
        }

        protected override void OnGraphicsRelease()
        {
            DeviceVK device = Device as DeviceVK;

            if (_set.Handle != 0)
            {
                Result r = device.VK.FreeDescriptorSets(device, Pool, 1, _set);
                r.Throw(device, () => "Failed to free descriptor set");
            }
        }

        internal DescriptorPoolVK Pool { get; }
    }
}
