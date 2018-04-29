using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class NormalsOverlay : DebugOverlayPage
    {
        public override void Render(SpriteFont font, RendererDX11 renderer, SpriteBatch batch, SceneRenderDataDX11 scene, IRenderSurface surface)
        {
            StartStep start = renderer.GetRenderStep<StartStep>();
            batch.Draw(start.Normals, surface.Viewport.Bounds, Color.White);
            batch.DrawString(font, $"Normals", new Vector2F(3, 3), Color.Yellow);
        }
    }
}
