using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal unsafe class ShaderPassVK : HlslPass
    {
        PipelineStateVK _state;

        internal ShaderPassVK(HlslShader material, string name = null) : 
            base(material, name)
        {
           
        }

        protected override void OnInitialize(ref ShaderPassParameters parameters)
        {
            _state = new PipelineStateVK(Device as DeviceVK, this, ref parameters);
        }

        protected override void OnGraphicsRelease()
        {
            _state.Dispose();

            base.OnGraphicsRelease();
        }
    }
}
