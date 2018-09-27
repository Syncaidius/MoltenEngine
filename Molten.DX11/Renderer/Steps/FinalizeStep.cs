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
        RenderCamera _camFinalize;
        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {
            _camFinalize = new RenderCamera(RenderCameraMode.Orthographic);
            UpdateSurfaces(renderer, width, height);
        }

        internal override void UpdateSurfaces(RendererDX11 renderer, int width, int height)
        {

        }

        public override void Dispose()
        {

        }

        internal override void Render(RendererDX11 renderer, RenderCamera camera, SceneRenderData sceneData, LayerRenderData<Renderable> layerData, Timing time, RenderChain.Link link)
        {
            switch (link.Chain.First.Step)
            {
                case StartStep start:
                    _camFinalize.OutputSurface = camera.OutputSurface;

                    Rectangle bounds = new Rectangle(0, 0, camera.OutputSurface.Width, camera.OutputSurface.Height);
                    GraphicsDeviceDX11 device = renderer.Device;
                    RenderSurfaceBase finalSurface = camera.OutputSurface as RenderSurfaceBase;
                    if (!camera.Flags.HasFlag(RenderCameraFlags.DoNotClear))
                        renderer.ClearIfFirstUse(device, finalSurface, sceneData.BackgroundColor);

                    device.SetRenderSurface(finalSurface, 0);
                    device.SetDepthSurface(null, GraphicsDepthMode.Disabled);
                    device.Rasterizer.SetViewports(camera.OutputSurface.Viewport);

                    renderer.SpriteBatcher.Draw(start.Scene, bounds, Vector2F.Zero, Vector2F.One, Color.White, 0, Vector2F.Zero, null);

                    StateConditions conditions = StateConditions.ScissorTest;
                    conditions |= camera.OutputSurface.SampleCount > 1 ? StateConditions.Multisampling : StateConditions.None;


                    renderer.Device.BeginDraw(conditions); // TODO correctly use pipe + conditions here.
                    renderer.SpriteBatcher.Flush(device, _camFinalize);
                    renderer.Device.EndDraw();
                    break;
            }
        }
    }
}
