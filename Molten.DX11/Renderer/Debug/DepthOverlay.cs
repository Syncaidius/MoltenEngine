using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class DepthOverlay : DebugOverlayPage
    {
        public override void Render(SpriteFont font, RendererDX11 renderer, SpriteBatch batch, SceneRenderDataDX11 scene, IRenderSurface surface)
        {
            StartStep start = renderer.GetRenderStep<StartStep>();
            batch.Draw(start.Depth, surface.Viewport.Bounds, surface.Viewport.Bounds, Color.White);
            batch.DrawString(font, $"Depth", new Vector2F(3, 3), Color.Yellow);
        }
    }
}
