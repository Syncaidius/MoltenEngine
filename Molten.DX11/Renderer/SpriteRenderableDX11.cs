using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class SpriteRenderableDX11 : Renderable, ISpriteRenderable
    {
        internal SpriteRenderableDX11(GraphicsDeviceDX11 device) : base(device) { }

        internal override void Render(GraphicsPipe pipe, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data)
        {
            renderer.SpriteBatcher.Begin(camera.OutputSurface.Viewport);
            renderer.SpriteBatcher.Draw(SpriteCache);
            Matrix4F wvp = data.RenderTransform * camera.ViewProjection;
            renderer.SpriteBatcher.End(pipe, ref wvp, camera.OutputSurface);
        }

        public SpriteBatchCache SpriteCache { get; set; }
    }
}
