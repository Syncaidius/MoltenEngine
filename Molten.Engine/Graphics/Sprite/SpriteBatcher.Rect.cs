using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract partial class SpriteBatcher
    {
        /// <summary>Adds an untextured rectangle to the <see cref="SpriteBatch"/>.</summary>
        /// <param name="destination">The rectangle defining the draw destination.</param>
        /// <param name="color">The color overlay/tiny of the sprite.</param>
        /// <param name="material">The material to apply to the rectangle. A value of null will use the default sprite-batch material.</param>
        public void DrawRect(RectangleF destination, Color color, IMaterial material = null)
        {
            DrawRect(destination, color, 0, Vector2F.Zero, material);
        }

        /// <summary>Adds an untextured rectangle to the <see cref="SpriteBatch"/>.</summary>
        /// <param name="destination">The rectangle defining the draw destination.</param>
        /// <param name="color">The color overlay/tiny of the sprite.</param>
        /// <param name="rotation">Rotation in radians.</param>
        /// <param name="origin">The origin, as a unit value. 1.0f will set the origin to the bottom-right corner of the sprite.
        /// 0.0f will set the origin to the top-left. The origin acts as the center of the sprite.</param>
        /// <param name="material">The material to use when rendering the sprite.</param>
        /// <param name="arraySlice">The texture array slice containing the source texture.</param>
        public void DrawRect(RectangleF destination, Color color, float rotation, Vector2F origin, IMaterial material = null)
        {
            ref SpriteItem item = ref GetItem();
            item.Texture = null;
            item.Material = material;
            item.Format = SpriteFormat.Sprite;

            item.Vertex.Position = destination.TopLeft;
            item.Vertex.Rotation = rotation;
            item.Vertex.ArraySlice = 0;
            item.Vertex.Size = destination.Size;
            item.Vertex.Color = color;
            item.Vertex.Origin = origin;
            //item.Vertex.UV = new Vector4F(); // Unused
        }

        /// <summary>
        /// Draws a rectangular outline composed of 4 lines.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <param name="color">The color.</param>
        /// <param name="thickness">The thickness.</param>
        public void DrawRectOutline(RectangleF rect, Color color, float thickness)
        {
            float halfThick = thickness / 2f;

            DrawLine(new Vector2F(rect.Left - halfThick, rect.Top), new Vector2F(rect.Right + halfThick, rect.Top), color, thickness); // Top
            DrawLine(new Vector2F(rect.Left - halfThick, rect.Bottom), new Vector2F(rect.Right + halfThick, rect.Bottom), color, thickness); // Bottom
            DrawLine(new Vector2F(rect.Right, rect.Top + halfThick), new Vector2F(rect.Right, rect.Bottom - halfThick), color, thickness); // Right
            DrawLine(new Vector2F(rect.Left, rect.Top + halfThick), new Vector2F(rect.Left, rect.Bottom - halfThick), color, thickness); // Left
        }

        public void DrawRoundedRect(RectangleF dest, Color color, float radius, IMaterial material = null)
        {
            DrawRoundedRect(dest, color, 0, Vector2F.Zero, radius, material);
        }

        public void DrawRoundedRect(RectangleF dest, Color color, float rotation, Vector2F origin, float radius, IMaterial material = null)
        {
            if (radius <= 0)
            {
                DrawRect(dest, color, rotation, origin, material);
                return;
            }

            // TODO add support for rotation and origin

            Vector2F tl = dest.TopLeft + radius;
            Vector2F tr = dest.TopRight + new Vector2F(-radius, radius);
            Vector2F br = dest.BottomRight - radius;
            Vector2F bl = dest.BottomLeft + new Vector2F(radius, -radius);

            float innerWidth = dest.Width - (radius * 2);
            float innerHeight = dest.Height - (radius * 2);
            RectangleF t = new RectangleF(tl.X, dest.Top, innerWidth, radius);
            RectangleF b = new RectangleF(tl.X, dest.Bottom - radius, innerWidth, radius);
            RectangleF c = new RectangleF(dest.X, tl.Y, dest.Width, innerHeight);

            DrawCircle(tl, radius, MathHelper.PiHalf * 3, MathHelper.TwoPi, color);
            DrawCircle(tr, radius, 0, MathHelper.PiHalf, color);
            DrawCircle(br, radius, MathHelper.PiHalf, MathHelper.Pi, color);
            DrawCircle(bl, radius, MathHelper.Pi, MathHelper.PiHalf * 3, color);

            DrawRect(t, color, material);
            DrawRect(b, color, material);
            DrawRect(c, color, material);
        }
    }
}
