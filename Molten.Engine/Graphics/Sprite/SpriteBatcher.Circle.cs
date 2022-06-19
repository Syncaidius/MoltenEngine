using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract partial class SpriteBatcher
    {
        public void DrawCircle(ref Circle c, Color color, float rotation)
        {
            DrawCircle(ref c, color, Vector2F.Zero, rotation, RectangleF.Empty);
        }

        public void DrawCircle(ref Circle c, Color color, float rotation, ITexture2D texture = null, IMaterial material = null, float arraySlice = 0)
        {
            RectangleF src;
            if (texture != null)
                src = new RectangleF(0, 0, texture.Width, texture.Height);
            else
                src = RectangleF.Empty;

            DrawCircle(ref c, color, Vector2F.Zero, rotation, src, texture, material, arraySlice);
        }

        public void DrawCircle(ref Circle c, Color color, float rotation, RectangleF source, ITexture2D texture = null, IMaterial material = null, float arraySlice = 0)
        {
            DrawCircle(ref c, color, Vector2F.Zero, rotation, source, texture, material, arraySlice);
        }

        /// <summary>
        /// Draws a circle with the specified radius.
        /// </summary>
        /// <param name="center">The position of the circle center.</param>
        /// <param name="radius">The radius, in radians.</param>
        /// <param name="startAngle">The start angle of the circle, in radians. This is useful when drawing a partial-circle.</param>
        /// <param name="endAngle">The end angle of the circle, in radians. This is useful when drawing a partial-circle</param>
        /// <param name="col">The color of the circle.</param>
        /// <param name="sides">The number of sides for every 6.28319 radians (360 degrees). A higher value will produce a smoother edge. The minimum value is 3.</param>
        public void DrawCircle(ref Circle c, Color color, Vector2F origin, float rotation, RectangleF source, ITexture2D texture = null, IMaterial material = null, float arraySlice = 0)
        {
            RectangleF bounds = new RectangleF()
            {
                X = c.Center.X - c.Radius,
                Y = c.Center.Y - c.Radius,
                Width = c.Radius * 2,
                Height = c.Radius * 2,
            };

            DrawInternal(texture, source, bounds.TopLeft, bounds.Size, color, rotation, origin, null, SpriteFormat.Ellipse, arraySlice);
        }
    }
}
