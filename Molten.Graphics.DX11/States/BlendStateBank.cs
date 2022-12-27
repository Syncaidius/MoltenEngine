using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal class BlendStateBank : GraphicsStateBank<GraphicsBlendState, BlendPreset>
    {
        internal BlendStateBank(DeviceDX11 device)
        {
            AddPreset(BlendPreset.Default, new GraphicsBlendState(device));

            // Additive blending preset.
            GraphicsBlendState state = new GraphicsBlendState(device, new RenderTargetBlendDesc1()
            {
                SrcBlend = Blend.One,
                DestBlend = Blend.One,
                BlendOp = BlendOp.Add,
                SrcBlendAlpha = Blend.One,
                DestBlendAlpha = Blend.One,
                BlendOpAlpha = BlendOp.Add,
                RenderTargetWriteMask = (byte)ColorWriteEnable.All,
                BlendEnable = 1,
                LogicOp = LogicOp.Noop,
                LogicOpEnable = 0
            })
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,

            };
            AddPreset(BlendPreset.Additive, state);

            // Pre-multiplied alpha
            state = new GraphicsBlendState(device, new RenderTargetBlendDesc1()
            {
                SrcBlend = Blend.SrcAlpha,
                DestBlend = Blend.InvSrcAlpha,
                BlendOp = BlendOp.Add,
                SrcBlendAlpha = Blend.InvDestAlpha,
                DestBlendAlpha = Blend.One,
                BlendOpAlpha = BlendOp.Add,
                RenderTargetWriteMask = (byte)ColorWriteEnable.All,
                BlendEnable = 1,
                LogicOp = LogicOp.Noop,
                LogicOpEnable = 0
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
