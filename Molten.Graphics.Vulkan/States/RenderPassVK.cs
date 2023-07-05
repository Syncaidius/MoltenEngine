using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal class RenderPassVK : GraphicsObject
    {
        RenderPass _handle;

        public RenderPassVK(DeviceVK device) : 
            base(device)
        {
            RenderPassCreateInfo info = new RenderPassCreateInfo()
            {
                SType = StructureType.RenderPassCreateInfo,
            };
        }

        protected unsafe override void OnGraphicsRelease()
        {
            if (_handle.Handle != 0)
            {
                DeviceVK device = Device as DeviceVK;
                device.VK.DestroyRenderPass(device, _handle, null);
                _handle = new RenderPass();
            }
        }

        internal RenderPass Handle => _handle;
    }
}
