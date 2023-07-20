using Silk.NET.Core.Native;
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
            State = new PipelineStateVK(Device as DeviceVK, this, ref parameters);
        }

        protected override void OnGraphicsRelease()
        {
            State?.Dispose();

            base.OnGraphicsRelease();
        }

        internal PipelineStateVK State { get; private set; }
    }
}
