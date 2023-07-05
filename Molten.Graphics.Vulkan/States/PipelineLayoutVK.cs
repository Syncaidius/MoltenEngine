using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal class PipelineLayoutVK : GraphicsObject
    {
        PipelineLayout _handle;

        public unsafe PipelineLayoutVK(DeviceVK device, params DescriptorSetLayoutVK[] layouts) :
            base(device)
        {
            DescriptorSetLayout* descLayouts = stackalloc DescriptorSetLayout[layouts.Length];
            for (int i = 0; i < layouts.Length; i++)
                descLayouts[i] = layouts[i].Handle;

            PipelineLayoutCreateInfo info = new PipelineLayoutCreateInfo()
            {
                SType = StructureType.PipelineLayoutCreateInfo,
                Flags = PipelineLayoutCreateFlags.None,
                PushConstantRangeCount = 0,
                PPushConstantRanges = null,
                PSetLayouts = descLayouts,
                SetLayoutCount = (uint)layouts.Length,
                PNext = null
            };

            Result r = Result.Success;
            fixed (PipelineLayout* ptrHandle = &_handle)
                r = device.VK.CreatePipelineLayout(device, info, null, ptrHandle);

            r.Throw(device, () => "Failed to create pipeline layout.");
        }

        protected override unsafe void OnGraphicsRelease()
        {
            if (_handle.Handle != 0)
            {
                DeviceVK device = Device as DeviceVK;
                device.VK.DestroyPipelineLayout(device, _handle, null);
                _handle = new PipelineLayout();
            }
        }

        internal PipelineLayout Handle => _handle;
    }
}
