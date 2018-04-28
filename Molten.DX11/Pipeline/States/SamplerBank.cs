using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class SamplerBank : GraphicsStateBank<ShaderSampler, SamplerPreset>
    {
        internal SamplerBank()
        {
            AddPreset(SamplerPreset.Default, new ShaderSampler()
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
