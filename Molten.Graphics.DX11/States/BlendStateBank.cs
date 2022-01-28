using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class BlendStateBank : GraphicsStateBank<GraphicsBlendState, BlendPreset>
    {
        internal BlendStateBank(Device device)
        {
            AddPreset(BlendPreset.Default, new GraphicsBlendState(device));

            // Additive blending preset.
            GraphicsBlendState state = new GraphicsBlendState(device, new RenderTargetBlendDesc1()
            {
                SrcBlend = Blend.BlendOne,
                DestBlend = Blend.BlendOne,
                BlendOp = BlendOp.BlendOpAdd,
                SrcBlendAlpha = Blend.BlendOne,
                DestBlendAlpha = Blend.BlendOne,
                BlendOpAlpha = BlendOp.BlendOpAdd,
                RenderTargetWriteMask = (byte)ColorWriteEnable.ColorWriteEnableAll,
                BlendEnable = 1,
            })
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,

            };
            AddPreset(BlendPreset.Additive, state);

            // Pre-multiplied alpha
            state = new GraphicsBlendState(device, new RenderTargetBlendDesc1()
            {
                SrcBlend = Blend.BlendSrcAlpha,
                DestBlend = Blend.BlendInvSrcAlpha,
                BlendOp = BlendOp.BlendOpAdd,

                SrcBlendAlpha = Blend.BlendInvDestAlpha,
                DestBlendAlpha = Blend.BlendOne,
                BlendOpAlpha = BlendOp.BlendOpAdd,

                RenderTargetWriteMask = (byte)ColorWriteEnable.ColorWriteEnableAll,
                BlendEnable = 1,
            })
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
            };
            AddPreset(BlendPreset.PreMultipliedAlpha, state);
        }

        internal override GraphicsBlendState GetPreset(BlendPreset value)
        {
            return _presets[(int)value];
        }
    }

    public enum BlendPreset
    {
        /// <summary>The default blend mode.</summary>
        Default = 0,

        /// <summary>Additive blending mode.</summary>
        Additive = 1,

        /// <summary>Pre-multiplied alpha blending mode.</summary>
        PreMultipliedAlpha = 2,
    }
}
