using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// The forward-rendering step.
    /// </summary>
    internal class ForwardStep : RenderStepBase
    {
        RenderSurface _surfaceScene;
        DepthSurface _surfaceDepth;

        internal override void Initialize(RendererDX11 renderer)
        {
            _surfaceScene = renderer.GetSurface<RenderSurface>(MainSurfaceType.Scene);
            _surfaceDepth = renderer.GetDepthSurface();
        }

        public override void Dispose()
        { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, RenderChain.Context context, Timing time)
        {
            GraphicsDeviceDX11 device = renderer.Device;

            if (!camera.Flags.HasFlag(RenderCameraFlags.DoNotClear))
                renderer.ClearIfFirstUse(device, _surfaceScene, context.Scene.BackgroundColor);

            device.SetRenderSurface(_surfaceScene, 0);
            device.DepthSurface = _surfaceDepth;
            device.Rasterizer.SetViewports(camera.OutputSurface.Viewport);
            device.Rasterizer.SetScissorRectangle(camera.OutputSurface.Viewport.Bounds);

            StateConditions conditions = StateConditions.ScissorTest; // TODO expand
            conditions |= camera.OutputSurface.SampleCount > 1 ? StateConditions.Multisampling : StateConditions.None;

            device.BeginDraw(conditions);
            renderer.RenderSceneLayer(device, context.Layer, camera);
            device.EndDraw();
        }
    }
}
