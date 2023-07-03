using System;
using System.Collections.Generic;
using System.Linq;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    internal unsafe class DescriptorSetLayoutVK : GraphicsObject
    {
        DescriptorSetLayoutBinding* _ptrBindings;
        DescriptorSetLayout _layout;

        public DescriptorSetLayoutVK(DeviceVK device, ShaderPassVK pass) : 
            base(device)
        {
            List<DescriptorSetLayoutBinding> layoutBindings = new List<DescriptorSetLayoutBinding>();

            for (uint slotID = 0; slotID < pass.Parent.Resources.Length; slotID++)
            {
                ShaderResourceVariable variable = pass.Parent.Resources[slotID];
                DescriptorSetLayoutBinding binding = new DescriptorSetLayoutBinding()
                {
                    DescriptorType = GetDescriptorType(variable),
                    Binding = slotID,
                    DescriptorCount = 1,
                    PImmutableSamplers = null,
                    StageFlags = GetShaderStageFlags(pass, slotID),
                };

                layoutBindings.Add(binding);
            }

            _ptrBindings = EngineUtil.AllocArray<DescriptorSetLayoutBinding>((uint)layoutBindings.Count);
            for (int i = 0; i < layoutBindings.Count; i++)
                _ptrBindings[i] = layoutBindings[i];

            DescriptorSetLayoutCreateInfo layoutInfo = new DescriptorSetLayoutCreateInfo()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                Flags = DescriptorSetLayoutCreateFlags.None,
                BindingCount = (uint)layoutBindings.Count,
                PBindings = _ptrBindings,
                PNext = null,
            };

            Result r = Result.Success;
            fixed (DescriptorSetLayout* ptrLayout = &_layout)
                r = device.VK.CreateDescriptorSetLayout(device, layoutInfo, null, ptrLayout);

            if (r != Result.Success)
                r.Throw(device, () => $"Failed to create descriptor set layout for shader pass '{pass.Name}' of shader '{pass.Parent.Name}'");
        }

        private ShaderStageFlags GetShaderStageFlags(ShaderPassVK pass, uint slotID)
        {
            ShaderStageFlags flags = ShaderStageFlags.None;

            foreach(ShaderType type in ShaderPassVK.ShaderTypes)
            {
                ShaderComposition comp = pass[type];
                if (comp != null && comp.ResourceIds.Contains(slotID))
                    flags |= ShaderPassVK.ShaderStageLookup[type];
            }

            return flags;
        }

        private DescriptorType GetDescriptorType(ShaderVariable variable)
        {
            switch (variable)
            {
                case ShaderResourceVariable<ITexture1D>:
                case ShaderResourceVariable<ITexture2D>:
                case ShaderResourceVariable<ITexture3D>:
                case ShaderResourceVariable<ITextureCube>:
                    return DescriptorType.SampledImage;

                case ShaderResourceVariable<GraphicsBuffer>:
                case RWVariable<GraphicsBuffer>:
                    return DescriptorType.StorageBuffer;

                case RWVariable<ITexture1D>:
                case RWVariable<ITexture2D>:
                case RWVariable<ITexture3D>:
                case RWVariable<ITextureCube>:
                    return DescriptorType.StorageImage;

                case ShaderSamplerVariable:
                    return DescriptorType.Sampler;

                default:
                    throw new NotSupportedException("Unsupported shader resource variable type");
            }
        }

        protected override void OnGraphicsRelease()
        {
            DeviceVK device = Device as DeviceVK;

            if(_layout.Handle != 0)
                device.VK.DestroyDescriptorSetLayout(device, _layout, null);

            EngineUtil.Free(ref _ptrBindings);
        }

        public DescriptorSetLayout Handle => _layout;
    }
}
