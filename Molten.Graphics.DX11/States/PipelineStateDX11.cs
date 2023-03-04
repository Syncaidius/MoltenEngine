using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public class PipelineStateDX11 : GraphicsState
    {
        internal PipelineStateDX11(DeviceDX11 device, ref GraphicsStateParameters parameters, string name = null) :
            base(device)
        {
            Name = name;

            // Check for unsupported features
            if (parameters.RasterizerDiscardEnabled)
                throw new NotSupportedException($"DirectX 11 mode does not support enabling of '{nameof(GraphicsStateParameters.RasterizerDiscardEnabled)}'");

            BlendState = new BlendStateDX11(device, ref parameters);
            BlendState = device.CacheObject(BlendState.Desc, BlendState);

            RasterizerState = new RasterizerStateDX11(device, ref parameters);
            RasterizerState = device.CacheObject(RasterizerState.Desc, RasterizerState);

            DepthState = new DepthStateDX11(device, ref parameters);
            DepthState = device.CacheObject(DepthState.Desc, DepthState);

            Topology = parameters.Topology.ToApi();
        }

        public override void GraphicsRelease() { }

        protected override void OnApply(GraphicsCommandQueue cmd) { }

        internal DepthStateDX11 DepthState { get; }

        internal RasterizerStateDX11 RasterizerState { get; }

        internal BlendStateDX11 BlendState { get; }

        internal D3DPrimitiveTopology Topology { get; }
    }
}
