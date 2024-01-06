using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal class DescriptorPoolVK : GraphicsObject<DeviceVK>
{
    List<DescriptorPoolSize> _sizes;
    DescriptorPool _handle;

    internal DescriptorPoolVK(DeviceVK device, DescriptorPoolSize[] sizes) :
        base(device)
    {
        _sizes = new List<DescriptorPoolSize>(sizes);
    }

    internal DescriptorPoolVK(DeviceVK device, DescriptorSetLayoutVK layout, uint maxSets) :
        base(device)
    {
        _sizes = new List<DescriptorPoolSize>();
        foreach (DescriptorSetLayoutBinding binding in layout.Bindings)
            AddPooling(binding.DescriptorType, binding.DescriptorCount);

        Build(maxSets);
    }

    /// <summary>
    /// Adds pooling to the current <see cref="DescriptorPoolVK"/> for the given descriptor type.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="poolSize"></param>
    /// <exception cref="InvalidOperationException"></exception>
    internal void AddPooling(DescriptorType type, uint poolSize)
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
            Result r = Device.VK.CreateDescriptorPool(Device, &info, null, ptrHandle);
            r.Throw(Device, () => "Failed to create descriptor pool");
        }
    }

    internal unsafe DescriptorSetVK Allocate(ShaderPassVK pass, DescriptorSetLayoutVK layout)
    {
        return Allocate(pass, [layout])[0];
    }

    internal unsafe DescriptorSetVK[] Allocate(ShaderPassVK pass, DescriptorSetLayoutVK[] layouts)
    {
        DescriptorSetVK[] sets = new DescriptorSetVK[layouts.Length];
        DescriptorSet* ptrSets = stackalloc DescriptorSet[layouts.Length];
        DescriptorSetLayout* ptrLayouts = stackalloc DescriptorSetLayout[layouts.Length];

        for(int i = 0; i < layouts.Length; i++)
            ptrLayouts[i] = layouts[i].Handle;

        DescriptorSetAllocateInfo info = new DescriptorSetAllocateInfo()
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = _handle,
            DescriptorSetCount = (uint)layouts.Length,
            PSetLayouts = ptrLayouts,
            PNext = null,
        };

        Result r = Device.VK.AllocateDescriptorSets(Device, &info, ptrSets);
        r.Throw(Device, () => "Failed to allocate descriptor sets");

        for (int i = 0; i < sets.Length; i++)
            sets[i] = new DescriptorSetVK(pass, this, layouts[i], ref ptrSets[i]);

        return sets;
    }

    internal unsafe void Free(DescriptorSetVK set)
    {
        Result r = Device.VK.FreeDescriptorSets(Device, _handle, 1, set);
        r.Throw(Device, () => "Failed to free descriptor set");
    }

    protected override unsafe void OnGraphicsRelease()
    {
        if(_handle.Handle != 0)
            Device.VK.DestroyDescriptorPool(Device, _handle, null);
    }

    public static implicit operator DescriptorPool(DescriptorPoolVK pool)
    {
        return pool._handle;
    }
}
