using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public class BlendStateBank : GraphicsStateBank<GraphicsBlendState, BlendPreset>
    {
        internal BlendStateBank(GraphicsDevice device)
        {
            GraphicsBlendState state = device.CreateBlendState();
            AddPreset(BlendPreset.Default, state);

            // Additive blending preset.
            GraphicsBlendState.RenderSurfaceBlend sBlend = device.GetDefaultSurfaceBlend();
            sBlend.SrcBlend = BlendType.One;
            sBlend.DestBlend = BlendType.One;
            sBlend.BlendOp = BlendOperation.Add;
            sBlend.SrcBlendAlpha = BlendType.One;
            sBlend.DestBlendAlpha = BlendType.One;
            sBlend.BlendOpAlpha = BlendOperation.Add;
            sBlend.RenderTargetWriteMask = (byte)ColorWriteEnable.All;
            sBlend.BlendEnable = 1;
            sBlend.LogicOp = LogicOperation.Noop;
            sBlend.LogicOpEnable = false;

            state = device.CreateBlendState(sBlend);
            state.AlphaToCoverageEnable = false;
            state.IndependentBlendEnable = false;
            AddPreset(BlendPreset.Additive, state);

            // Pre-multiplied alpha
            sBlend = device.GetDefaultSurfaceBlend();
            sBlend.SrcBlend = BlendType.SrcAlpha;
            sBlend.DestBlend = BlendType.InvSrcAlpha;
            sBlend.BlendOp = BlendOperation.Add;
            sBlend.SrcBlendAlpha = BlendType.InvDestAlpha;
            sBlend.DestBlendAlpha = BlendType.One;
            sBlend.BlendOpAlpha = BlendOperation.Add;
            sBlend.RenderTargetWriteMask = (byte)ColorWriteEnable.All;
            sBlend.BlendEnable = 1;
            sBlend.LogicOp = LogicOperation.Noop;
            sBlend.LogicOpEnable = false;

            state = device.CreateBlendState(sBlend);
            state.AlphaToCoverageEnable = false;
            state.IndependentBlendEnable = false;
            AddPreset(BlendPreset.PreMultipliedAlpha, state);
        }

        public override GraphicsBlendState GetPreset(BlendPreset value)
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
