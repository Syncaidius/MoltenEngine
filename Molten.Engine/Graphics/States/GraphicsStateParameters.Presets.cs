using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public partial struct GraphicsStateParameters
    {
        public void ApplyPreset(GraphicsStatePreset preset)
        {
            switch (preset)
            {
                default:
                case GraphicsStatePreset.Default:
                    ApplyBlendPreset(BlendPreset.Default);
                    ApplyRasterizerPreset(RasterizerPreset.Default);
                    ApplyDepthPreset(DepthStencilPreset.Default);
                    break;
            }
        }

        public void ApplyBlendPreset(BlendPreset preset, int blendSlot = -1)
        {
            BlendFactor = new Color4(1, 1, 1, 1);
            BlendSampleMask = 0xffffffff;
            AlphaToCoverageEnable = false;
            IndependentBlendEnable = false;

            int slot = blendSlot >= 0 ? blendSlot : 0;
            ref SurfaceBlend b = ref Surface0;

            b.SrcBlend = BlendType.One;
            b.DestBlend = BlendType.Zero;
            b.BlendOp = BlendOperation.Add;
            b.SrcBlendAlpha = BlendType.One;
            b.DestBlendAlpha = BlendType.Zero;
            b.BlendOpAlpha = BlendOperation.Add;
            b.RenderTargetWriteMask = ColorWriteFlags.All;
            b.BlendEnable = true;
            b.LogicOp = LogicOperation.Noop;
            b.LogicOpEnable = false;

            switch (preset)
            {
                case BlendPreset.Additive:
                    b.SrcBlend = BlendType.One;
                    b.DestBlend = BlendType.One;
                    b.BlendOp = BlendOperation.Add;
                    b.SrcBlendAlpha = BlendType.One;
                    b.DestBlendAlpha = BlendType.One;
                    b.BlendOpAlpha = BlendOperation.Add;
                    b.RenderTargetWriteMask = ColorWriteFlags.All;
                    b.BlendEnable = true;
                    b.LogicOp = LogicOperation.Noop;
                    b.LogicOpEnable = false;
                    AlphaToCoverageEnable = false;
                    IndependentBlendEnable = false;
                    break;

                case BlendPreset.PreMultipliedAlpha:
                    b.SrcBlend = BlendType.SrcAlpha;
                    b.DestBlend = BlendType.InvSrcAlpha;
                    b.BlendOp = BlendOperation.Add;
                    b.SrcBlendAlpha = BlendType.InvDestAlpha;
                    b.DestBlendAlpha = BlendType.One;
                    b.BlendOpAlpha = BlendOperation.Add;
                    b.RenderTargetWriteMask = ColorWriteFlags.All;
                    b.BlendEnable = true;
                    b.LogicOp = LogicOperation.Noop;
                    b.LogicOpEnable = false;
                    AlphaToCoverageEnable = false;
                    IndependentBlendEnable = false;
                    break;
            }

            // Apply the blend to the other slots, unless specified.
            if (blendSlot < 0)
            {
                for (int i = 0; i < MAX_SURFACES; i++)
                {
                    if (i != slot)
                        this[i] = b;
                }
            }
        }

        public void ApplyDepthPreset(DepthStencilPreset preset)
        {
            // Based on the default DX11 values: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/ns-d3d11-d3d11_depth_stencil_desc
            // Revert to defaults first
            IsDepthEnabled = true;
            DepthWriteEnabled = true;
            DepthComparison = ComparisonFunction.Less;
            IsStencilEnabled = true;
            StencilReadMask = 255;
            StencilWriteMask = 255;
            DepthFrontFace.Comparison = ComparisonFunction.Always;
            DepthFrontFace.DepthFail = DepthStencilOperation.Keep;
            DepthFrontFace.StencilPass = DepthStencilOperation.Keep;
            DepthFrontFace.StencilFail = DepthStencilOperation.Keep;
            DepthBackFace = DepthFrontFace;

            // Now apply customizations
            switch (preset)
            {
                case DepthStencilPreset.DefaultNoStencil:
                    IsStencilEnabled = false;
                    break;

                case DepthStencilPreset.ZDisabled:
                    IsStencilEnabled = false;
                    IsDepthEnabled = false;
                    DepthWriteEnabled = false;
                    break;
            }
        }

        public void ApplyRasterizerPreset(RasterizerPreset preset)
        {
            // Revert to defaults first
            Fill = RasterizerFillingMode.Solid;
            Cull = RasterizerCullingMode.Back;
            IsFrontCounterClockwise = false;
            DepthBias = 0;
            DepthBiasEnabled = false;
            SlopeScaledDepthBias = 0.0f;
            DepthBiasClamp = 0.0f;
            IsDepthClipEnabled = true;
            IsScissorEnabled = false;
            IsMultisampleEnabled = false;
            IsAALineEnabled = false;
            ConservativeRaster = ConservativeRasterizerMode.Off;
            ForcedSampleCount = 0;

            // Now apply customizations
            switch (preset)
            {
                case RasterizerPreset.Wireframe:
                    Fill = RasterizerFillingMode.Wireframe;
                    break;

                case RasterizerPreset.ScissorTest:
                    IsScissorEnabled = true;
                    break;

                case RasterizerPreset.NoCulling:
                    Cull = RasterizerCullingMode.None;
                    break;

                case RasterizerPreset.DefaultMultisample:
                    IsMultisampleEnabled = true;
                    break;

                case RasterizerPreset.ScissorTestMultisample:
                    IsScissorEnabled = true;
                    IsMultisampleEnabled = true;
                    break;
            }
        }
    }

    public enum GraphicsStatePreset
    {
        Default = 0,
    }
}
