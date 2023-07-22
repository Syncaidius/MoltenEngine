using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Silk.NET.Vulkan;

namespace Molten.Graphics.Vulkan
{
    public class SamplerVK : ShaderSampler
    {
        Sampler _handle;

        public unsafe SamplerVK(GraphicsDevice device, ref ShaderSamplerParameters parameters) : 
            base(device, ref parameters)
        {
            SamplerCustomBorderColorCreateInfoEXT* customColor = null;

            // If any of the address modes use border, we need to provide the custom border color extension info.
            if (parameters.AddressU == SamplerAddressMode.Border
                || parameters.AddressV == SamplerAddressMode.Border
                || parameters.AddressW == SamplerAddressMode.Border)
            {
                customColor = EngineUtil.Alloc<SamplerCustomBorderColorCreateInfoEXT>();
                customColor[0] = new SamplerCustomBorderColorCreateInfoEXT()
                {
                    SType = StructureType.SamplerCustomBorderColorCreateInfoExt,
                    CustomBorderColor = Unsafe.As<Color4, ClearColorValue>(ref parameters.BorderColor),
                    Format = Format.R32G32B32A32Sfloat,
                    PNext = null
                };
            }

            SamplerCreateInfo info = new SamplerCreateInfo()
            {
                SType = StructureType.SamplerCreateInfo,
                CompareEnable = parameters.IsComparison,
                AnisotropyEnable = parameters.MaxAnisotropy > 0,
                MaxAnisotropy = Math.Max(1, parameters.MaxAnisotropy),
                MinLod = parameters.MinMipMapLod,
                MaxLod = parameters.MaxMipMapLod,
                MipLodBias = parameters.LodBias,
                AddressModeU = GetAddressMode(parameters.AddressU),
                AddressModeV = GetAddressMode(parameters.AddressV),
                AddressModeW = GetAddressMode(parameters.AddressW),
                MinFilter = GetFilter(parameters.MinFilter),
                MagFilter = GetFilter(parameters.MagFilter),
                BorderColor = BorderColor.FloatCustomExt,
                Flags = SamplerCreateFlags.None,
                CompareOp = GetCompareOp(parameters.Comparison),
                MipmapMode = GetMipMode(parameters.MipFilter),
                PNext = customColor,
                UnnormalizedCoordinates = false, // TODO Is this point sampling?
            };
        }

        private CompareOp GetCompareOp(ComparisonMode mode)
        {
            switch (mode)
            {
                case ComparisonMode.Less: return CompareOp.Less;
                case ComparisonMode.LessEqual: return CompareOp.LessOrEqual;
                case ComparisonMode.Greater: return CompareOp.Greater;
                case ComparisonMode.GreaterEqual: return CompareOp.GreaterOrEqual;
                case ComparisonMode.Equal: return CompareOp.Equal;
                case ComparisonMode.NotEqual: return CompareOp.NotEqual;
                case ComparisonMode.Always: return CompareOp.Always;
                case ComparisonMode.Never: return CompareOp.Never;
                default: throw new Exception("Unsupported comparison mode: " + mode);
            }
        }

        private Silk.NET.Vulkan.SamplerAddressMode GetAddressMode(SamplerAddressMode mode)
        {
            switch (mode)
            {
                case SamplerAddressMode.Wrap:
                    return Silk.NET.Vulkan.SamplerAddressMode.Repeat;

                case SamplerAddressMode.Mirror:
                    return Silk.NET.Vulkan.SamplerAddressMode.MirroredRepeat;

                case SamplerAddressMode.Clamp:
                    return Silk.NET.Vulkan.SamplerAddressMode.ClampToEdge;

                case SamplerAddressMode.MirrorOnce:
                    return Silk.NET.Vulkan.SamplerAddressMode.MirrorClampToEdge;

                case SamplerAddressMode.Border:
                    return Silk.NET.Vulkan.SamplerAddressMode.ClampToBorder;

                default:
                    throw new Exception("Unsupported sampler address mode: " + mode);
            }
        }

        private Filter GetFilter(SamplerFilter filter)
        {
            switch (filter)
            {
                case SamplerFilter.Linear:
                    return Filter.Linear;

                case SamplerFilter.Point:
                    return Filter.Nearest;

                default:
                    throw new Exception($"Unsupported sampler filter: {filter}");
            }
        }

        private SamplerMipmapMode GetMipMode(SamplerFilter filter)
        {
            switch (filter)
            {
                case SamplerFilter.Linear:
                    return SamplerMipmapMode.Linear;

                case SamplerFilter.Point:
                    return SamplerMipmapMode.Nearest;

                default:
                    throw new Exception($"Unsupported mip-map mode: {filter}");
            }
        }

        protected unsafe override void OnGraphicsRelease()
        {
            if (_handle.Handle != 0)
            {
                DeviceVK device = Device as DeviceVK;
                device.VK.DestroySampler(device, _handle, null);
            }
        }

        public static implicit operator Sampler(SamplerVK sampler) => sampler._handle;
    }
}
