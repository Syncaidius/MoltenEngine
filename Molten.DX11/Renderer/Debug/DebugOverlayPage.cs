using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class DebugOverlayPage
    {
        public abstract void Render(SpriteFont font, RendererDX11 renderer, SpriteBatch sb, SceneRenderDataDX11 scene, IRenderSurface surface);
    }
}
