using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan;

internal class DescriptorSetVK : GraphicsObject
{
    DescriptorSetLayoutVK _layout;
    DescriptorSet _set;

    internal DescriptorSetVK(ShaderPassVK pass, DescriptorPoolVK pool, DescriptorSetLayoutVK layout, ref DescriptorSet set) : 
        base(pass.Device)
    {
        Pool = pool;
        _set = set;
        _layout = layout;
    }

    internal unsafe void SetDescriptor(BufferVK buffer)
    {
        DescriptorBufferInfo info = new DescriptorBufferInfo()
        {
            Buffer = *buffer.Handle.NativePtr,
            Offset = 0,
            Range = buffer.SizeInBytes,
        };

        DescriptorType type;
        switch(buffer.BufferType)
        {
            case GraphicsBufferType.Constant:
                type = DescriptorType.UniformBuffer;
                break;

            case GraphicsBufferType.Structured:
                type = DescriptorType.StorageBuffer;
                break;

            default:
                throw new NotImplementedException();
        }

        WriteDescriptorSet write = new WriteDescriptorSet()
        {
            SType = StructureType.WriteDescriptorSet,
            PNext = null,
            DescriptorCount = 1,
            DescriptorType = type,
            PBufferInfo = &info,
            PImageInfo = null,
            PTexelBufferView = null,
            DstArrayElement = 0,
            DstBinding = 0,
            DstSet = _set,
        };

        throw new NotImplementedException("Not finished");
    }

    internal unsafe void SetDescriptor(TextureVK texture, SamplerVK sampler)
    {
        DescriptorImageInfo info = new DescriptorImageInfo()
        {
            ImageView = *texture.Handle.SubHandle.ViewPtr,
            ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
            Sampler = sampler
        };

        DescriptorType type = DescriptorType.SampledImage;
        if (info.ImageView.Handle != 0)
        {
            if (info.Sampler.Handle != 0)
                type = DescriptorType.CombinedImageSampler;
        }

        WriteDescriptorSet write = new WriteDescriptorSet()
        {
            SType = StructureType.WriteDescriptorSet,
            PNext = null,
            DescriptorCount = 1,
            DescriptorType = type,
            PBufferInfo = null,
            PImageInfo = &info,
            PTexelBufferView = null,
            DstArrayElement = 0,
            DstBinding = 0,
            DstSet = _set,
        };

        throw new NotImplementedException("Not finished");
    }

    protected override void OnGraphicsRelease() => 
        Pool.Free(this);

    public static implicit operator DescriptorSet(DescriptorSetVK set) => 
        set._set;

    internal DescriptorPoolVK Pool { get; }
}
