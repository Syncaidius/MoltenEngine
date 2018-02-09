using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class DebugBuffersPage : DebugOverlayPage
    {
        public override void Render(ISpriteFont font, RendererDX11 renderer, SpriteBatch sb, Timing time, IRenderSurface surface)
        {
            int width = 300;
            int height = 32;
            Rectangle dest = new Rectangle()
            {
                X = surface.Width - width,
                Y = height,
                Width = width,
                Height = height,
            };

            DrawBar("Static Vertex", font, sb, renderer.StaticVertexBuffer, dest);
            dest.Y += height + 3; DrawBar("Dynamic Vertex", font, sb, renderer.DynamicVertexBuffer, dest);
        }

        private void DrawBar(string label, ISpriteFont font, SpriteBatch sb, GraphicsBuffer buffer, Rectangle destination)
        {
            sb.Draw(destination, Color.Green);
        }
    }
}
