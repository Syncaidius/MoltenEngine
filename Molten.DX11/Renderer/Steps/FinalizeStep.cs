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
        internal override void Initialize(RendererDX11 renderer, int width, int height)
        {
            UpdateSurfaces(renderer, width, height);
        }

        internal override void UpdateSurfaces(RendererDX11 renderer, int width, int height)
        {

        }

        public override void Dispose()
        {

        }

        internal override void Render(RendererDX11 renderer, SceneRenderData<Renderable> scene, Timing time, RenderChain.Link link)
        {
            switch (link.Chain.First.Step)
            {
                case StartStep start:
                    Matrix4F spriteProj = Matrix4F.Identity;
                    Matrix4F spriteView = Matrix4F.OrthoOffCenterLH(0, scene.FinalSurface.Width, -scene.FinalSurface.Height, 0, 0, 1);
                    Matrix4F spriteViewProj = Matrix4F.Multiply(spriteView, spriteProj);

                    Rectangle bounds = new Rectangle(0, 0, scene.FinalSurface.Width, scene.FinalSurface.Height);
                    GraphicsDeviceDX11 device = renderer.Device;
                    RenderSurfaceBase finalSurface = scene.FinalSurface as RenderSurfaceBase;
                    if (!scene.HasFlag(SceneRenderFlags.DoNotClear))
                        renderer.ClearIfFirstUse(device, finalSurface, scene.BackgroundColor);

                    device.SetRenderSurface(finalSurface, 0);
                    device.SetDepthSurface(null, GraphicsDepthMode.Disabled);
                    device.Rasterizer.SetViewports(scene.FinalSurface.Viewport);

                    renderer.SpriteBatcher.Begin(scene.FinalSurface.Viewport);
                    renderer.SpriteBatcher.Draw(start.Scene, bounds, bounds, Color.White, 0, Vector2F.Zero, null);

                    StateConditions conditions = StateConditions.ScissorTest;
                    conditions |= scene.FinalSurface.SampleCount > 1 ? StateConditions.Multisampling : StateConditions.None;

                    renderer.Device.BeginDraw(conditions); // TODO correctly use pipe + conditions here.
                    renderer.SpriteBatcher.End(device, ref spriteViewProj, finalSurface);
                    renderer.Device.EndDraw();
                    break;
            }
        }
    }
}
