using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal unsafe class ShaderPassVK : HlslPass
    {
        DescriptorPoolVK _descPool;
        DescriptorSetVK _descSet;

        internal ShaderPassVK(HlslShader material, string name = null) :
            base(material, name)
        { }

        protected override void OnInitialize(ref ShaderPassParameters parameters)
        {
            DeviceVK device = Device as DeviceVK;
            State = new PipelineStateVK(device, this, ref parameters);
            DescriptorLayout = new DescriptorSetLayoutVK(device, this);

            _descPool = new DescriptorPoolVK(device, DescriptorLayout, 5);
            _descSet = _descPool.Allocate(this, DescriptorLayout);
        }

        protected override void OnGraphicsRelease()
        {
            _descSet?.Dispose();    
            _descPool?.Dispose();
            DescriptorLayout?.Dispose();
            State?.Dispose();

            base.OnGraphicsRelease();
        }

        internal PipelineStateVK State { get; private set; }

        internal DescriptorSetLayoutVK DescriptorLayout { get; private set; }

        internal DescriptorSetVK DescriptorSet => _descSet;
    }
}
