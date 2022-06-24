using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract partial class SpriteBatcher
    {
        public void DrawRoundedRect(RectangleF dest, Color color, float radius, IMaterial material = null)
        {
            DrawRoundedRect(dest, 0, Vector2F.Zero, radius, material);
        }

        public void DrawRoundedRect(RectangleF dest, float radius, IMaterial material = null)
        {
            DrawRoundedRect(dest, 0, Vector2F.Zero, radius, material);
        }

        public void DrawRoundedRect(RectangleF dest, float rotation, Vector2F origin, float radius, IMaterial material = null)
        {
            if (radius <= 0)
            {
                DrawRect(dest, rotation, origin, material);
                return;
            }

            SpriteStyle userStyle = _style;

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

            Circle ctl = new Circle(bl, radius, MathHelper.PiHalf * 3, MathHelper.TwoPi);
            Circle ctr = new Circle(tl, radius, 0, MathHelper.PiHalf);
            Circle cbr = new Circle(tr, radius, MathHelper.PiHalf, MathHelper.Pi);
            Circle cbl = new Circle(br, radius, MathHelper.Pi, MathHelper.PiHalf * 3);

            SpriteStyle style = _style;
            SpriteStyle fillStyle = style;
            fillStyle.Thickness = 0;

            SetStyle(ref fillStyle);
            DrawCircle(ref ctl);
            DrawCircle(ref ctr);
            DrawCircle(ref cbr);
            DrawCircle(ref cbl);

            DrawRect(t, 0, material);
            DrawRect(b, 0, material);
            DrawRect(c, 0, material);

            if (style.Thickness > 0)
            {
                SetStyle(ref style);
                DrawCircle(ref ctl);
                DrawCircle(ref ctr);
                DrawCircle(ref cbr);
                DrawCircle(ref cbl);

                style.Thickness /= 2; 
                SetStyle(ref style);
                DrawLine(new Vector2F(dest.Left, dest.Top + radius), new Vector2F(dest.Left, dest.Bottom - radius));
                DrawLine(new Vector2F(dest.Right, dest.Top + radius), new Vector2F(dest.Right, dest.Bottom - radius));
                DrawLine(new Vector2F(dest.Left + radius, dest.Top), new Vector2F(dest.Right - radius, dest.Top));
                DrawLine(new Vector2F(dest.Left + radius, dest.Bottom), new Vector2F(dest.Right - radius, dest.Bottom));
            }

            SetStyle(ref userStyle);
        }

        /*public void DrawRoundedRect(RectangleF dest, Color color, float rotation, Vector2F origin, RoundedCornerInfo corners, IMaterial material = null)
        {
            if (!corners.HasRounded())
            {
                DrawRect(dest, color, rotation, origin, material);
                return;
            }

            if (corners.OneRadius())
            {
                DrawRoundedRect(dest, color, rotation, origin, corners.TopLeftRadius, material);
                return;
            }

            // TODO add support for rotation and origin

            Vector2F tl = dest.TopLeft + corners.TopLeftRadius;
            Vector2F tr = dest.TopRight + new Vector2F(-corners.TopRightRadius, corners.TopRightRadius);
            Vector2F br = dest.BottomRight - corners.BottomRightRadius;
            Vector2F bl = dest.BottomLeft + new Vector2F(corners.BottomLeftRadius, -corners.BottomLeftRadius);

            float topWidth = dest.Width - corners.TopLeftRadius - corners.TopRightRadius;
            float bottomWidth = dest.Width - corners.BottomLeftRadius - corners.BottomRightRadius;
            float leftHeight = dest.Height - corners.TopLeftRadius - corners.BottomLeftRadius;
            float rightHeight = dest.Height - corners.TopRightRadius - corners.BottomRightRadius;

            DrawCircle(tl, corners.TopLeftRadius, MathHelper.PiHalf * 3, MathHelper.TwoPi, color);
            DrawCircle(tr, corners.TopRightRadius, 0, MathHelper.PiHalf, color);
            DrawCircle(br, corners.BottomRightRadius, MathHelper.PiHalf, MathHelper.Pi, color);
            DrawCircle(bl, corners.BottomLeftRadius, MathHelper.Pi, MathHelper.PiHalf * 3, color);

            // Draw left edge
            if (corners.LeftOneRadius())
            {
                float leftWidth = corners.TopLeftRadius;
                DrawRect(new RectangleF(dest.X, tl.Y, leftWidth, leftHeight), color, material);
            }
            else
            {
                if (corners.TopLeftRadius < corners.BottomLeftRadius)
                {
                    float dif = corners.BottomLeftRadius - corners.TopLeftRadius;
                    float leftHeight2 = leftHeight + corners.TopLeftRadius;
                    DrawRect(new RectangleF(dest.X, tl.Y, corners.TopLeftRadius, leftHeight), color, material);
                    DrawRect(new RectangleF(dest.X + corners.TopLeftRadius, dest.Y, dif, leftHeight2), color, material);
                }
                else
                {
                    float dif = corners.TopLeftRadius - corners.BottomLeftRadius;
                    float leftHeight2 = leftHeight + corners.BottomLeftRadius;
                    DrawRect(new RectangleF(dest.X, tl.Y, corners.BottomLeftRadius, leftHeight), color, material);
                    DrawRect(new RectangleF(dest.X + corners.BottomLeftRadius, dest.Y + corners.TopLeftRadius, dif, leftHeight2), color, material);
                }
            }
        }*/
    }
}
