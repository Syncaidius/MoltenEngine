using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal unsafe class ShaderPassVK : HlslPass
    {

        internal ShaderPassVK(HlslShader material, string name = null) : 
            base(material, name)
        {
           
        }

        protected override void OnInitialize(ref ShaderPassParameters parameters)
        {
            DeviceVK device = Device as DeviceVK;
            State = new PipelineStateVK(device, this, ref parameters);
            DescriptorLayout = new DescriptorSetLayoutVK(device, this);
        }

        protected override void OnGraphicsRelease()
        {
            DescriptorLayout?.Dispose();
            State?.Dispose();

            base.OnGraphicsRelease();
        }

        internal PipelineStateVK State { get; private set; }

        internal DescriptorSetLayoutVK DescriptorLayout { get; private set; }
    }
}
