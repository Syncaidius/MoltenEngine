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
            DrawRoundedRect(dest, rotation, origin, new RoundedCornerInfo(radius), material);
        }

        public void DrawRoundedRect(RectangleF dest, float rotation, Vector2F origin, RoundedCornerInfo corners, IMaterial material = null)
        {
            if (!corners.HasRounded())
            {
                DrawRect(dest, rotation, origin, material);
                return;
            }

            SpriteStyle userStyle = _style;
            SpriteStyle fillStyle = userStyle;
            SpriteStyle circleStyle = userStyle;
            fillStyle.Thickness = 0;

            // TODO add support for rotation and origin

            Vector2F tl = dest.TopLeft + corners.TopLeft;
            Vector2F tr = dest.TopRight + new Vector2F(-corners.TopRight, corners.TopRight);
            Vector2F br = dest.BottomRight - corners.BottomRight;
            Vector2F bl = dest.BottomLeft + new Vector2F(corners.BottomLeft, -corners.BottomLeft);

            float topWidth = dest.Width - corners.TopLeft - corners.TopRight;
            float bottomWidth = dest.Width - corners.BottomLeft - corners.BottomRight;
            float leftHeight = dest.Height - corners.TopLeft - corners.BottomLeft;
            float rightHeight = dest.Height - corners.TopRight - corners.BottomRight;

            Circle ctl = new Circle(tl, corners.TopLeft, 0, MathHelper.PiHalf);
            Circle ctr = new Circle(tr, corners.TopRight, MathHelper.PiHalf, MathHelper.Pi);
            Circle cbr = new Circle(br, corners.BottomRight, MathHelper.Pi, MathHelper.PiHalf * 3);
            Circle cbl = new Circle(bl, corners.BottomLeft, MathHelper.PiHalf * 3, MathHelper.TwoPi);

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

            // Draw left edge
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

            // Draw right edge
            if (corners.RightHasRadius())
            {
                if (corners.RightSameRadius())
                {
                    DrawRect(new RectangleF(dest.Right - corners.TopRight, tl.Y, corners.TopRight, dest.Height - (corners.TopRight * 2)));
                    rightEdgeWidth = corners.TopRight;
                }
                else
                {

                    if (corners.TopRight < corners.BottomRight)
                    {
                        rightEdgeWidth = corners.BottomRight;
                        float dif = corners.BottomRight - corners.TopRight;
                        float rightHeight2 = rightHeight + corners.TopRight;
                        DrawRect(new RectangleF(dest.Right - corners.TopRight, tl.Y, corners.TopRight, rightHeight), 0, material);
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

                Vector2F l = new Vector2F(dest.Left + lo, dest.Top + corners.TopLeft);
                Vector2F r = new Vector2F(dest.Right - lo, dest.Top + corners.TopRight);
                Vector2F t = new Vector2F(dest.Left + corners.TopLeft, dest.Top + lo);
                Vector2F b = new Vector2F(dest.Left + corners.BottomLeft, dest.Bottom);

                SetStyle(ref style);
                DrawLine(l, l + new Vector2F(0, leftHeight)); // Left
                DrawLine(r, r + new Vector2F(0, rightHeight)); // Right
                DrawLine(t, t + new Vector2F(topWidth, 0)); // Top
                DrawLine(b, b + new Vector2F(bottomWidth, 0)); // Bottom
            }

            SetStyle(ref userStyle);
        }
    }
}
