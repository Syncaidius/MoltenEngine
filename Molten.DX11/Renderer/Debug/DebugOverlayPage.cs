using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class DebugOverlayPage
    {
        public abstract void Render(ISpriteFont font, RendererDX11 renderer, SpriteBatch sb, Timing time, IRenderSurface surface);
    }
}
