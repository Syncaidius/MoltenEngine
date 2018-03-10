using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class DebugBuffersPage : DebugOverlayPage
    {
        Color _bgColor = new Color(40, 40, 40, 240);
        Color _segColor = Color.Lime;

        public override void Render(ISpriteFont font, RendererDX11 renderer, SpriteBatchDX11 sb, Timing time, IRenderSurface surface)
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

            sb.DrawString(font, "Buffer segmentation: ", new Vector2F(dest.X - 10, dest.Y), Color.White);
            dest.Y += 22; DrawBar("Static Vertex", font, sb, renderer.StaticVertexBuffer, dest);
            dest.Y += height + 2; DrawBar("Dynamic Vertex", font, sb, renderer.DynamicVertexBuffer, dest);

            dest.Y += height + 5; DrawBar("Static Index", font, sb, renderer.StaticIndexBuffer, dest);
            dest.Y += height + 5; DrawBar("Dynamic Index", font, sb, renderer.DynamicIndexBuffer, dest);
        }

        private void DrawBar(string label, ISpriteFont font, SpriteBatchDX11 sb, GraphicsBuffer buffer, Rectangle destination)
        {
            sb.DrawRect(destination, _bgColor);

            double capacity = buffer.ByteCapacity;
            int used = 0;
            Rectangle segDest = destination;
            BufferSegment seg = buffer.FirstSegment;

            do
            {
                if (!seg.IsFree)
                {
                    double usePercent = seg.ByteOffset / capacity;
                    segDest.X = destination.X + (int)(usePercent * destination.Width);
                    segDest.Width = (int)((seg.ByteCount / capacity) * destination.Width);
                    sb.DrawRect(segDest, _segColor);
                    used += seg.ByteCount;
                }

                seg = seg.Next;
            } while (seg != null);

            Vector2F tPos = new Vector2F(destination.X, destination.Y);
            double percentUsed = (used / capacity) * 100.0;

            sb.DrawString(font, $"{label}: {used}/{capacity} ({percentUsed.ToString("N2")}%) bytes used", tPos, Color.White);
        }
    }
}
