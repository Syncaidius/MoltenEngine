using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class DebugBuffersPage : DebugOverlayPage
    {
        Color _bgColor = new Color(40, 40, 40, 230);
        Color _segColor = Color.Lime;

        public override void Render(ISpriteFont font, RendererDX11 renderer, SpriteBatch sb, Timing time, IRenderSurface surface)
        {
            int width = surface.Width / 2;
            int height = 32;
            Rectangle dest = new Rectangle()
            {
                X = surface.Width - width - 10,
                Y = 5,
                Width = width,
                Height = height,
            };

            sb.DrawString(font, "Buffer segmentation: ", new Vector2(dest.X - 10, dest.Y), Color.White);
            dest.Y += 22; DrawBar("Static Vertex", font, sb, renderer.StaticVertexBuffer, dest);
            dest.Y += height + 2; DrawBar("Dynamic Vertex", font, sb, renderer.DynamicVertexBuffer, dest);

            dest.Y += height + 5; DrawBar("Static Index", font, sb, renderer.StaticIndexBuffer, dest);
            dest.Y += height + 5; DrawBar("Dynamic Index", font, sb, renderer.DynamicIndexBuffer, dest);
        }

        private void DrawBar(string label, ISpriteFont font, SpriteBatch sb, GraphicsBuffer buffer, Rectangle destination)
        {
            sb.Draw(destination, _bgColor);

            int capacity = buffer.ByteCapacity;
            int used = 0;
            Rectangle segDest = destination;
            BufferSegment seg = buffer.FirstSegment;

            do
            {
                if (!seg.IsFree)
                {
                    float usePercent = (float)seg.ByteOffset / capacity;
                    segDest.X = destination.X + (int)(usePercent * destination.Width);
                    segDest.Width = (int)(((float)seg.ByteCount / capacity) * destination.Width);
                    sb.Draw(segDest, _segColor);
                    used += seg.ByteCount;
                }

                seg = seg.Next;
            } while (seg != null);

            Vector2 tPos = new Vector2(destination.X, destination.Y);
            float percentUsed = ((float)used / capacity) * 100;

            sb.DrawString(font, $"{label}: {used}/{capacity} ({percentUsed.ToString("N2")}%) bytes used", tPos, Color.White);
        }
    }
}
