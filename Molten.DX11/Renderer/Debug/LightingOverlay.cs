using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class LightingOverlay : DebugOverlayPage
    {
        public override void Render(SpriteFont font, RendererDX11 renderer, SpriteBatch batch, SceneRenderDataDX11 scene, IRenderSurface surface)
        {
            LightingStep lightStep = renderer.GetRenderStep<LightingStep>();
            batch.Draw(lightStep.Lighting, surface.Viewport.Bounds, Color.White);
            batch.DrawString(font, $"Lighting", new Vector2F(3, 3), Color.Yellow);
        }
    }
}
