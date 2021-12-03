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
        internal BlendStateBank(DeviceDX11 device)
        {
            AddPreset(BlendPreset.Default, new GraphicsBlendState(device));

            // Additive blending preset.
            GraphicsBlendState state = new GraphicsBlendState(device, new RenderTargetBlendDescription()
            {
                SourceBlend = Blend.BlendOne,
                DestinationBlend = Blend.BlendOne,
                BlendOperation = BlendOp.BlendOpAdd,
                SourceAlphaBlend = Blend.BlendOne,
                DestinationAlphaBlend = Blend.BlendOne,
                AlphaBlendOperation = BlendOp.BlendOpAdd,
                RenderTargetWriteMask = ColorWriteEnable.ColorWriteEnableAll,
                IsBlendEnabled = true,
            })
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,

            };
            AddPreset(BlendPreset.Additive, state);

            // Pre-multiplied alpha
            state = new GraphicsBlendState(device, new RenderTargetBlendDescription()
            {
                SourceBlend = Blend.BlendSrcAlpha,
                DestinationBlend = Blend.BlendInvSrcAlpha,
                BlendOperation = BlendOp.BlendOpAdd,

                SourceAlphaBlend = Blend.BlendInvDestAlpha,
                DestinationAlphaBlend = Blend.BlendOne,
                AlphaBlendOperation = BlendOp.BlendOpAdd,

                RenderTargetWriteMask = ColorWriteEnable.ColorWriteEnableAll,
                IsBlendEnabled = true,
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
