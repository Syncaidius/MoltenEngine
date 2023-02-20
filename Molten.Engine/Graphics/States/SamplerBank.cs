using System;

namespace Molten.Graphics
{
    public class SamplerBank : GraphicsStateBank<ShaderSampler, SamplerPreset>
    {
        internal SamplerBank(GraphicsDevice device)
        {
            ShaderSampler sampler = device.CreateSampler();


            AddPreset(SamplerPreset.Default, sampler);
        }

        public override void ApplyPreset(ShaderSampler sampler, SamplerPreset preset)
        {
            // Revert to default
            sampler.Filter = SamplerFilter.MinMagMipLinear;
            sampler.AddressU = SamplerAddressMode.Wrap;
            sampler.AddressV = SamplerAddressMode.Wrap;
            sampler.AddressW = SamplerAddressMode.Wrap;
            sampler.MinMipMapLod = float.MinValue;
            sampler.MaxMipMapLod = float.MaxValue;
            sampler.LodBias = 0f;
            sampler.MaxAnisotropy = 1;
            sampler.BorderColor = Color.White;
            sampler.Comparison = ComparisonMode.Never;

            // Now apply preset values.
            switch (preset)
            {
                case SamplerPreset.Clamp:
                    sampler.AddressU = SamplerAddressMode.Clamp;
                    sampler.AddressV = SamplerAddressMode.Clamp;
                    sampler.AddressW = SamplerAddressMode.Clamp;
                    break;
            }
        }
    }

    public enum SamplerPreset
    {
        /// <summary>The default blend mode. All address modes are set to <see cref="SamplerAddressMode.Wrap"/></summary>
        Default = 0,

        Clamp = 1,
    }
}
