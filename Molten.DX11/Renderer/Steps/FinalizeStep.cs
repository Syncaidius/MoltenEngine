using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class FinalizeStep : RenderStepBase
    {
        RenderCamera _orthoCamera;
        ObjectRenderData _dummyData;

        RenderSurface _surfaceScene;

        internal override void Initialize(RendererDX11 renderer)
        {
            _surfaceScene = renderer.GetSurface<RenderSurface>(MainSurfaceType.Scene);

            _dummyData = new ObjectRenderData();
            _orthoCamera = new RenderCamera(RenderCameraMode.Orthographic);
        }

        public override void Dispose()
        {

        }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, SceneRenderData sceneData, LayerRenderData<Renderable> layerData, Timing time, RenderChain.Link link)
        {
            _orthoCamera.OutputSurface = camera.OutputSurface;

            Rectangle bounds = new Rectangle(0, 0, camera.OutputSurface.Width, camera.OutputSurface.Height);
            GraphicsDeviceDX11 device = renderer.Device;
            RenderSurfaceBase finalSurface = camera.OutputSurface as RenderSurfaceBase;
            if (!camera.HasFlags(RenderCameraFlags.DoNotClear))
                renderer.ClearIfFirstUse(device, finalSurface, sceneData.BackgroundColor);

            device.SetRenderSurface(finalSurface, 0);
            device.DepthSurface = null;
            device.DepthWriteOverride = GraphicsDepthWritePermission.Disabled;
            device.Rasterizer.SetViewports(camera.OutputSurface.Viewport);
            device.Rasterizer.SetScissorRectangle(camera.OutputSurface.Viewport.Bounds);

            StateConditions conditions = StateConditions.ScissorTest;
            conditions |= camera.OutputSurface.SampleCount > 1 ? StateConditions.Multisampling : StateConditions.None;

            renderer.Device.BeginDraw(conditions); // TODO correctly use pipe + conditions here.
            renderer.SpriteBatcher.Draw(_surfaceScene, bounds, Vector2F.Zero, camera.OutputSurface.Viewport.Bounds.Size, Color.White, 0, Vector2F.Zero, null, 0);

            if (camera.HasFlags(RenderCameraFlags.ShowOverlay))
                renderer.Overlay.Render(time, renderer.SpriteBatcher, renderer.Profiler, sceneData.Profiler, camera);

            renderer.SpriteBatcher.Flush(device, _orthoCamera, _dummyData);
            renderer.Device.EndDraw();
        }
    }
}
