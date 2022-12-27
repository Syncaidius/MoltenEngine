namespace Molten.Graphics
{
    internal class SamplerBank : GraphicsStateBank<ShaderSampler, SamplerPreset>
    {
        internal SamplerBank(Device device)
        {
            AddPreset(SamplerPreset.Default, new ShaderSampler(device)
            {
                AddressU = SamplerAddressMode.Wrap,
                AddressV = SamplerAddressMode.Wrap,
                AddressW = SamplerAddressMode.Wrap,
            });
        }

        internal override ShaderSampler GetPreset(SamplerPreset value)
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
