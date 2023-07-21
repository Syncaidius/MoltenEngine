using System;
using System.Collections.Generic;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal class DescriptorPoolVK : GraphicsObject
    {
        List<DescriptorPoolSize> _sizes;
        DescriptorPool _handle;

        internal DescriptorPoolVK(DeviceVK device, DescriptorPoolSize[] sizes) : 
            base(device)
        {
            _sizes = new List<DescriptorPoolSize>(sizes);
        }

        internal void AddPool(DescriptorType type, uint poolSize)
        {
            if(_handle.Handle != 0)
                throw new InvalidOperationException("Cannot add a pool to a descriptor pool that has already been built.");

            _sizes.Add(new DescriptorPoolSize()
            {
                Type = type,
                DescriptorCount = poolSize,
            });
        }

        internal unsafe void Build(uint maxSets)
        {
            DeviceVK device = Device as DeviceVK;
            DescriptorPoolSize[] sizes = _sizes.ToArray();

            DescriptorPoolCreateInfo info = new DescriptorPoolCreateInfo()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PPoolSizes = EngineUtil.AllocArray<DescriptorPoolSize>((uint)_sizes.Count),
                PoolSizeCount = (uint)_sizes.Count,
                Flags = DescriptorPoolCreateFlags.None,
                MaxSets = maxSets,
                PNext = null
            };

            fixed (DescriptorPool* ptrHandle = &_handle)
            {
                Result r = device.VK.CreateDescriptorPool(device, &info, null, ptrHandle);
                r.Throw(device, () => "Failed to create descriptor pool");
            }
        }

        internal unsafe DescriptorSetVK[] Allocate(ShaderPassVK pass, ref DescriptorSetAllocateInfo info)
        {
            DeviceVK device = Device as DeviceVK;
            DescriptorSetVK[] sets = new DescriptorSetVK[info.DescriptorSetCount];
            DescriptorSet* ptrSets = stackalloc DescriptorSet[sets.Length];

            fixed(DescriptorSetAllocateInfo* ptrInfo = &info)
                device.VK.AllocateDescriptorSets(device, ptrInfo, ptrSets);

            for(int i = 0; i < sets.Length; i++)
                sets[i] = new DescriptorSetVK(pass, this, ref ptrSets[i]);

            return sets;
        }

        protected override unsafe void OnGraphicsRelease()
        {
            DeviceVK device = Device as DeviceVK;

            if(_handle.Handle != 0)
                device.VK.DestroyDescriptorPool(device, _handle, null);
        }

        public static implicit operator DescriptorPool(DescriptorPoolVK pool)
        {
            return pool._handle;
        }
    }
}
