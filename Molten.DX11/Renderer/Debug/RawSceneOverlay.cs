using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class RawSceneOverlay : DebugOverlayPage
    {
        public override void Render(SpriteFont font, RendererDX11 renderer, SpriteBatch batch, SceneRenderData scene, IRenderSurface surface)
        {
            StartStep start = renderer.GetRenderStep<StartStep>();
            batch.Draw(start.Scene, surface.Viewport.Bounds, surface.Viewport.Bounds, Color.White);
            batch.DrawString(font, $"Raw Scene", new Vector2F(3, 3), Color.Yellow);
        }
    }
}
