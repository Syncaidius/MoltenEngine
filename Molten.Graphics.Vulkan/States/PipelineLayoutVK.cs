using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal class PipelineLayoutVK : GraphicsObject, IEquatable<PipelineLayoutVK>, IEquatable<PipelineLayoutCreateInfo>
    {
        PipelineLayout _handle;
        PipelineLayoutCreateInfo _info;
        List<DescriptorSetLayoutVK> _layouts;

        public unsafe PipelineLayoutVK(DeviceVK device, params DescriptorSetLayoutVK[] layouts) :
            base(device)
        {
            _layouts = new List<DescriptorSetLayoutVK>(layouts);

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

        public override bool Equals(object obj) => obj switch
        {
            PipelineLayoutVK vk => Equals(vk),
            PipelineLayoutCreateInfo info => Equals(info),
            _ => false
        };

        public bool Equals(PipelineLayoutVK other)
        {
            return Equals(other._info);
        }

        public unsafe bool Equals(PipelineLayoutCreateInfo other)
        {
            bool equal = _info.SetLayoutCount == other.SetLayoutCount &&
                _info.PushConstantRangeCount == other.PushConstantRangeCount &&
                _info.Flags == other.Flags &&
                _info.SetLayoutCount == other.SetLayoutCount;

            if (!equal)
                return false;

            // Check if push ranges are equal.
            if (_info.PushConstantRangeCount != other.PushConstantRangeCount)
                return false;

            if (_info.PushConstantRangeCount > 0)
            {
                for (uint i = _info.PushConstantRangeCount = 0; i < _info.PushConstantRangeCount; i++)
                {
                    if (!PushRangesEqual(ref _info.PPushConstantRanges[i], ref other.PPushConstantRanges[i]))
                        return false;
                }
            }

            if(_info.SetLayoutCount != other.SetLayoutCount)
                return false;

            // Check if layouts are the same
            if (_info.SetLayoutCount > 0)
            { 
                for (int i = 0; i < _layouts.Count; i++)
                {
                    if (!_layouts[i].Equals(other.PSetLayouts[i]))
                        return false;
                }
            }

            return true;
        }

        private bool PushRangesEqual(ref PushConstantRange a, ref PushConstantRange b)
        {
            return a.Offset == b.Offset &&
                a.Size == b.Size &&
                a.StageFlags == b.StageFlags;
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

        public static implicit operator PipelineLayout(PipelineLayoutVK layout) => layout._handle;

        internal PipelineLayout Handle => _handle;
    }
}
