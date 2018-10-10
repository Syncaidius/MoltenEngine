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
        ObjectRenderData _dummyData;
        RenderCamera _orthoCamera;

        RenderSurface _surfaceScene;
        DepthSurface _surfaceDepth;

        internal override void Initialize(RendererDX11 renderer)
        {
            _surfaceScene = renderer.GetSurface<RenderSurface>(MainSurfaceType.Scene);
            _surfaceDepth = renderer.GetDepthSurface();

            _dummyData = new ObjectRenderData();
            _orthoCamera = new RenderCamera(RenderCameraMode.Orthographic);
        }

        public override void Dispose()
        { }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, SceneRenderData sceneData, LayerRenderData<Renderable> layerData, Timing time, RenderChain.Link link)
        {
            RenderSurfaceBase rs = null;
            GraphicsDeviceDX11 device = renderer.Device;

            _orthoCamera.OutputSurface = camera.OutputSurface;


            if (!camera.Flags.HasFlag(RenderCameraFlags.DoNotClear))
                renderer.ClearIfFirstUse(device, rs, sceneData.BackgroundColor);

            device.SetRenderSurface(_surfaceScene, 0);
            device.SetRenderSurface(rs, 0);
            device.DepthSurface = _surfaceDepth;
            device.Rasterizer.SetViewports(rs.Viewport);
            device.Rasterizer.SetScissorRectangle(rs.Viewport.Bounds);

            StateConditions conditions = StateConditions.ScissorTest; // TODO expand
            conditions |= rs.SampleCount > 1 ? StateConditions.Multisampling : StateConditions.None;

            device.BeginDraw(conditions);
            renderer.RenderSceneLayer(device, layerData, camera);

            if (camera.HasFlags(RenderCameraFlags.ShowOverlay))
                renderer.Overlay.Render(time, renderer.SpriteBatcher, renderer.Profiler, sceneData.Profiler, camera);
            renderer.SpriteBatcher.Flush(device, _orthoCamera, _dummyData);
            device.EndDraw();
        }
    }
}
