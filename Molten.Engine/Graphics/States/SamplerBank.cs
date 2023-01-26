namespace Molten.Graphics
{
    public class SamplerBank : GraphicsStateBank<ShaderSampler, SamplerPreset>
    {
        internal SamplerBank(GraphicsDevice device)
        {
            ShaderSampler sampler = device.CreateSampler();
            sampler.AddressU = SamplerAddressMode.Wrap;
            sampler.AddressV = SamplerAddressMode.Wrap;
            sampler.AddressW = SamplerAddressMode.Wrap;

            AddPreset(SamplerPreset.Default, sampler);
        }

        public override ShaderSampler GetPreset(SamplerPreset value)
        {
            return _presets[(int)value];
        }
    }

    public enum SamplerPreset
    {
        /// <summary>The default blend mode. All address modes are set to <see cref="SamplerAddressMode.Wrap"/></summary>
        Default = 0,
    }
}
