using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public class SamplerVK : ShaderSampler
    {
        Sampler _handle;

        public SamplerVK(GraphicsDevice device, ref ShaderSamplerParameters parameters) : 
            base(device, ref parameters)
        {
            SamplerCreateInfo info = new SamplerCreateInfo()
            {
                SType = StructureType.SamplerCreateInfo,
                
            };

           // DirectX Point-sampling is equivilent to enabling un
        }

        protected unsafe override void OnGraphicsRelease()
        {
            if (_handle.Handle != 0)
            {
                DeviceVK device = Device as DeviceVK;
                device.VK.DestroySampler(device, _handle, null);
            }
        }

        public static implicit operator Sampler(SamplerVK sampler) => sampler._handle;
    }
}
