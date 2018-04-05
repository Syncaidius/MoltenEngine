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

        internal override void Render(RendererDX11 renderer, SceneRenderDataDX11 scene, Timing time, RenderChain.Link link)
        {
            switch (link.Chain.First.Step)
            {
                case StartStep start:
                    Matrix4F spriteProj = Matrix4F.Identity;
                    Matrix4F spriteView = Matrix4F.OrthoOffCenterLH(0, scene.FinalSurface.Width, -scene.FinalSurface.Height, 0, 0, 1);
                    Matrix4F spriteViewProj = Matrix4F.Multiply(spriteView, spriteProj);

                    Rectangle bounds = new Rectangle(0, 0, scene.FinalSurface.Width, scene.FinalSurface.Height);
                    GraphicsDevice device = renderer.Device;
                    if (!scene.HasFlag(SceneRenderFlags.DoNotClear))
                        renderer.ClearIfFirstUse(scene.FinalSurface, () => scene.FinalSurface.Clear(scene.BackgroundColor));

                    // Clear the depth surface if it hasn't already been cleared
                    if (scene.FinalDepthSurface != null)
                        renderer.ClearIfFirstUse(scene.FinalDepthSurface, () => scene.FinalDepthSurface.Clear(DepthClearFlags.Depth | DepthClearFlags.Stencil));

                    device.SetRenderSurface(scene.FinalSurface, 0);
                    device.SetDepthSurface(scene.FinalDepthSurface, GraphicsDepthMode.Enabled);
                    device.DepthStencil.SetPreset(DepthStencilPreset.Default);
                    device.Rasterizer.SetViewports(scene.FinalSurface.Viewport);

                    renderer.SpriteBatcher.Begin(scene.FinalSurface.Viewport);
                    renderer.SpriteBatcher.Draw(start.Scene, bounds, Color.White);
                    renderer.SpriteBatcher.Flush(device, ref spriteViewProj, scene.FinalSurface.SampleCount > 1);
                    break;
            }
        }
    }
}
