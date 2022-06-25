using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract partial class SpriteBatcher
    {
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


            SpriteStyle fillStyle = userStyle;
            SpriteStyle circleStyle = userStyle;
            fillStyle.Thickness = 0;

            SetStyle(ref circleStyle);
            DrawCircle(ref ctl);
            DrawCircle(ref ctr);
            DrawCircle(ref cbr);
            DrawCircle(ref cbl);

            SetStyle(ref fillStyle);
            DrawRect(t, 0, material);
            DrawRect(b, 0, material);
            DrawRect(c, 0, material);

            if (userStyle.Thickness > 0)
            {
                SpriteStyle style = userStyle;
                style.PrimaryColor = Color.Transparent;

                SetStyle(ref style);

                style.PrimaryColor = style.SecondaryColor;
                style.Thickness /= 2;
                float lo = 0.5f * style.Thickness; // Line offset

                SetStyle(ref style);
                DrawLine(new Vector2F(dest.Left + lo, dest.Top + radius), new Vector2F(dest.Left + lo, dest.Bottom - radius));
                DrawLine(new Vector2F(dest.Right - lo, dest.Top + radius), new Vector2F(dest.Right - lo, dest.Bottom - radius));
                DrawLine(new Vector2F(dest.Left + radius, dest.Top + lo), new Vector2F(dest.Right - radius, dest.Top + lo));
                DrawLine(new Vector2F(dest.Left + radius, dest.Bottom - lo), new Vector2F(dest.Right - radius, dest.Bottom - lo));
            }

            SetStyle(ref userStyle);
        }

        public void DrawRoundedRect(RectangleF dest, float rotation, Vector2F origin, RoundedCornerInfo corners, IMaterial material = null)
        {
            if (!corners.HasRounded())
            {
                DrawRect(dest, rotation, origin, material);
                return;
            }

            if (corners.OneRadius())
            {
                DrawRoundedRect(dest, rotation, origin, corners.TopLeft, material);
                return;
            }

            SpriteStyle userStyle = _style;

            // TODO add support for rotation and origin

            Vector2F tl = dest.TopLeft + corners.TopLeft;
            Vector2F tr = dest.TopRight + new Vector2F(-corners.TopRight, corners.TopRight);
            Vector2F br = dest.BottomRight - corners.BottomRight;
            Vector2F bl = dest.BottomLeft + new Vector2F(corners.BottomLeft, -corners.BottomLeft);

            float leftHeight = dest.Height - corners.TopLeft - corners.BottomLeft;
            float rightHeight = dest.Height - corners.TopRight - corners.BottomRight;

            Circle ctl = new Circle(tl, corners.TopLeft, 0, MathHelper.PiHalf);
            Circle ctr = new Circle(tr, corners.TopRight, MathHelper.PiHalf, MathHelper.Pi);
            Circle cbr = new Circle(br, corners.BottomRight, MathHelper.Pi, MathHelper.PiHalf * 3);
            Circle cbl = new Circle(bl, corners.BottomLeft, MathHelper.PiHalf * 3, MathHelper.TwoPi);


            SpriteStyle fillStyle = userStyle;
            SpriteStyle circleStyle = userStyle;
            fillStyle.Thickness = 0;

            SetStyle(ref circleStyle);
            if (corners.TopLeft > 0)
                DrawCircle(ref ctl);

            if (corners.TopRight > 0)
                DrawCircle(ref ctr);

            if (corners.BottomRight > 0)
                DrawCircle(ref cbr);

            if (corners.BottomLeft > 0)
                DrawCircle(ref cbl);

            SetStyle(ref fillStyle);
            float leftEdgeWidth = 0;
            float rightEdgeWidth = 0;

            if (corners.LeftHasRadius())
            {
                if (corners.LeftSameRadius())
                {
                    DrawRect(new RectangleF(dest.X, tl.Y, corners.TopLeft, dest.Height - (corners.TopLeft * 2)));
                    leftEdgeWidth = corners.TopLeft;
                }
                else
                {

                    if (corners.TopLeft < corners.BottomLeft)
                    {
                        leftEdgeWidth = corners.BottomLeft;
                        float dif = corners.BottomLeft - corners.TopLeft;
                        float leftHeight2 = leftHeight + corners.TopLeft;
                        DrawRect(new RectangleF(dest.X, tl.Y, corners.TopLeft, leftHeight), 0, material);
                        DrawRect(new RectangleF(dest.X + corners.TopLeft, dest.Y, dif, leftHeight2), 0, material);
                    }
                    else
                    {
                        leftEdgeWidth = corners.TopLeft;
                        float dif = corners.TopLeft - corners.BottomLeft;
                        float leftHeight2 = leftHeight + corners.BottomLeft;
                        DrawRect(new RectangleF(dest.X, tl.Y, corners.BottomLeft, leftHeight), 0, material);
                        DrawRect(new RectangleF(dest.X + corners.BottomLeft, dest.Y + corners.TopLeft, dif, leftHeight2), 0, material);
                    }
                }
            }

            if (corners.RightHasRadius())
            {
                if (corners.RightSameRadius())
                {
                    DrawRect(new RectangleF(dest.Right, tl.Y, corners.TopRight, dest.Height - (corners.TopRight * 2)));
                    rightEdgeWidth = corners.TopRight;
                }
                else
                {

                    if (corners.TopRight < corners.BottomRight)
                    {
                        rightEdgeWidth = corners.BottomRight;
                        float dif = corners.BottomRight - corners.TopRight;
                        float rightHeight2 = rightHeight + corners.TopRight;
                        DrawRect(new RectangleF(dest.Right, tl.Y, corners.TopRight, rightHeight), 0, material);
                        DrawRect(new RectangleF(dest.Right - corners.BottomRight, dest.Y, dif, rightHeight2), 0, material);
                    }
                    else
                    {
                        rightEdgeWidth = corners.TopRight;
                        float dif = corners.TopRight - corners.BottomRight;
                        float rightHeight2 = rightHeight + corners.BottomRight;
                        DrawRect(new RectangleF(dest.Right - corners.BottomRight, tr.Y, corners.BottomRight, rightHeight), 0, material);
                        DrawRect(new RectangleF(dest.Right - corners.TopRight, dest.Y + corners.TopRight, dif, rightHeight2), 0, material);
                    }
                }
            }

            // Draw center
            RectangleF c = new RectangleF(dest.X + leftEdgeWidth, dest.Y, dest.Width - leftEdgeWidth - rightEdgeWidth, dest.Height);
            DrawRect(c, 0, material);

            if (userStyle.Thickness > 0)
            {
                SpriteStyle style = userStyle;
                style.PrimaryColor = Color.Transparent;

                SetStyle(ref style);

                style.PrimaryColor = style.SecondaryColor;
                style.Thickness /= 2;
                float lo = 0.5f * style.Thickness; // Line offset

                SetStyle(ref style);
                DrawLine(new Vector2F(dest.Left + lo, dest.Top + corners.TopLeft), new Vector2F(dest.Left + lo, dest.Bottom - corners.BottomLeft)); // Left
                DrawLine(new Vector2F(dest.Right - lo, dest.Top + corners.TopRight), new Vector2F(dest.Right - lo, dest.Bottom - corners.BottomRight)); // Right
                DrawLine(new Vector2F(dest.Left + corners.TopLeft, dest.Top + lo), new Vector2F(dest.Right - corners.TopRight, dest.Top + lo)); // Top
                DrawLine(new Vector2F(dest.Left + corners.BottomLeft, dest.Bottom - lo), new Vector2F(dest.Right - corners.BottomRight, dest.Bottom - lo)); // Bottom
            }

            SetStyle(ref userStyle);
        }
    }
}
