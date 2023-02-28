using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public class PipelineStateDX11 : GraphicsState
    {
        internal PipelineStateDX11(DeviceDX11 device, ref GraphicsStateParameters parameters, string name = null) :
            base(device)
        {
            Name = name;
            StructKey<DepthStencilDesc>  descDepth = new StructKey<DepthStencilDesc>(); // TODO get default
            StructKey<RasterizerDesc2> descRaster = new StructKey<RasterizerDesc2>();
            StructKey<BlendDesc1> descBlend = new StructKey<BlendDesc1>();

            // Populate blend description
            ref BlendDesc1 bDesc = ref descBlend.Value;
            bDesc.IndependentBlendEnable = parameters.IndependentBlendEnable ? 1 : 0;
            bDesc.AlphaToCoverageEnable = parameters.AlphaToCoverageEnable ? 1 : 0;

            for (int i = 0; i < GraphicsStateParameters.MAX_SURFACES; i++)
            {
                ref RenderTargetBlendDesc1 sBlend = ref bDesc.RenderTarget[i];
                GraphicsStateParameters.SurfaceBlend pBlend = parameters[i];

                sBlend.BlendEnable = pBlend.BlendEnable ? 1 : 0;
                sBlend.SrcBlend = (Blend)pBlend.SrcBlend;
                sBlend.SrcBlendAlpha = (Blend)pBlend.SrcBlendAlpha;
                sBlend.DestBlend = (Blend)pBlend.DestBlend;
                sBlend.DestBlendAlpha = (Blend)pBlend.DestBlendAlpha;
                sBlend.RenderTargetWriteMask = (byte)pBlend.RenderTargetWriteMask;
                sBlend.LogicOp = (LogicOp)pBlend.LogicOp;
                sBlend.LogicOpEnable = pBlend.LogicOpEnable ? 1 : 0;
                sBlend.BlendOp = (BlendOp)pBlend.BlendOp;
                sBlend.BlendOpAlpha = (BlendOp)pBlend.BlendOpAlpha;

                parameters[i] = pBlend;
            }

            // Populate rasterizer description
            ref RasterizerDesc2 raDesc = ref descRaster.Value;
            raDesc.MultisampleEnable = parameters.IsMultisampleEnabled ? 1 : 0;
            raDesc.DepthClipEnable = parameters.IsDepthClipEnabled ? 1 : 0;
            raDesc.AntialiasedLineEnable = parameters.IsAALineEnabled ? 1 : 0;
            raDesc.ScissorEnable = parameters.IsScissorEnabled ? 1 : 0;
            raDesc.FillMode = parameters.Fill.ToApi();
            raDesc.CullMode = parameters.Cull.ToApi();
            raDesc.DepthBias = parameters.DepthBiasEnabled ? parameters.DepthBias : 0;
            raDesc.DepthBiasClamp = parameters.DepthBiasEnabled ? parameters.DepthBiasClamp : 0;
            raDesc.SlopeScaledDepthBias = parameters.SlopeScaledDepthBias;
            raDesc.ConservativeRaster = (ConservativeRasterizationMode)parameters.ConservativeRaster;
            raDesc.ForcedSampleCount = parameters.ForcedSampleCount;
            raDesc.FrontCounterClockwise = parameters.IsFrontCounterClockwise ? 1 : 0;

            // Check for unsupported features
            if (parameters.RasterizerDiscardEnabled)
                throw new NotSupportedException($"DirectX 11 mode does not support enabling of '{nameof(GraphicsStateParameters.RasterizerDiscardEnabled)}'");

            // Populate depth-stencil description
            ref DepthStencilDesc dDesc = ref descDepth.Value;
            dDesc.DepthEnable = parameters.IsDepthEnabled ? 1 : 0;
            dDesc.DepthFunc = (ComparisonFunc)parameters.DepthComparison;
            dDesc.DepthWriteMask = parameters.DepthWriteEnabled ? DepthWriteMask.All : DepthWriteMask.Zero;
            dDesc.StencilWriteMask = parameters.StencilWriteMask;
            dDesc.StencilReadMask = parameters.StencilReadMask;
            
            uint stencilRef = parameters.DepthFrontFace.StencilReference > 0 ? parameters.DepthFrontFace.StencilReference : parameters.DepthBackFace.StencilReference;
            dDesc.FrontFace = new DepthStencilopDesc()
            {
                StencilDepthFailOp = (StencilOp)parameters.DepthFrontFace.DepthFail,
                StencilFailOp = (StencilOp)parameters.DepthFrontFace.StencilFail,
                StencilFunc = (ComparisonFunc)parameters.DepthFrontFace.Comparison,
                StencilPassOp = (StencilOp)parameters.DepthFrontFace.StencilPass,
            };
            dDesc.BackFace = new DepthStencilopDesc()
            {
                StencilDepthFailOp = (StencilOp)parameters.DepthBackFace.DepthFail,
                StencilFailOp = (StencilOp)parameters.DepthBackFace.StencilFail,
                StencilFunc = (ComparisonFunc)parameters.DepthBackFace.Comparison,
                StencilPassOp = (StencilOp)parameters.DepthBackFace.StencilPass,
            };

            BlendState = new BlendStateDX11(device, ref descBlend.Value, parameters.BlendFactor, parameters.BlendSampleMask);
            BlendState = device.CacheObject(descBlend, BlendState);

            RasterizerState = new RasterizerStateDX11(device, ref descRaster.Value);
            RasterizerState = device.CacheObject(descRaster, RasterizerState);

            DepthState = new DepthStateDX11(device, ref descDepth.Value, stencilRef);
            DepthState = device.CacheObject(descDepth, DepthState);

            // Dispose of struct keys now we're done with them.
            descDepth.Dispose();
            descRaster.Dispose();
            descBlend.Dispose();
        }

        public override void GraphicsRelease() { }

        protected override void OnApply(GraphicsCommandQueue cmd) { }

        internal DepthStateDX11 DepthState { get; }

        internal RasterizerStateDX11 RasterizerState { get; }

        internal BlendStateDX11 BlendState { get; }
    }
}
