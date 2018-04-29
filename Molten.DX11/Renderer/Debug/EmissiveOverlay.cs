using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class EmissiveOverlay : DebugOverlayPage
    {
        public override void Render(SpriteFont font, RendererDX11 renderer, SpriteBatch batch, SceneRenderDataDX11 scene, IRenderSurface surface)
        {
            StartStep start = renderer.GetRenderStep<StartStep>();
            batch.Draw(start.Emissive, surface.Viewport.Bounds, Color.White);
            batch.DrawString(font, $"Emissive", new Vector2F(3, 3), Color.Yellow);
        }
    }
}
