using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract partial class SpriteBatcher
    {
        /// <summary>
        /// Draws an ellipse with the specified radius values.
        /// </summary>
        /// <param name="e">The <see cref="Ellipse"/> to be drawn</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="sides">The number of sides for every 6.28319 radians (360 degrees). A higher value will produce a smoother edge. The minimum value is 3.</param>
        public void DrawEllipse(ref Ellipse e, Color color, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
        {
            RectangleF bounds = new RectangleF()
            {
                X = e.Center.X - e.RadiusX,
                Y = e.Center.Y - e.RadiusY,
                Width = e.RadiusX * 2,
                Height = e.RadiusY * 2,
            };

            RectangleF src = new RectangleF(0, 0, texture.Width, texture.Height);
            DrawInternal(texture, src, bounds.TopLeft, bounds.Size, color, 0, Vector2F.Zero, material, SpriteFormat.Ellipse, arraySlice, false);
        }

        /// <summary>
        /// Draws an ellipse with the specified radius values.
        /// </summary>
        /// <param name="e">The <see cref="Ellipse"/> to be drawn</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="sides">The number of sides for every 6.28319 radians (360 degrees). A higher value will produce a smoother edge. The minimum value is 3.</param>
        public void DrawEllipseOutline(ref Ellipse e, Color color, float thickness, IMaterial material = null)
        {
            RectangleF bounds = new RectangleF()
            {
                X = e.Center.X - e.RadiusX,
                Y = e.Center.Y - e.RadiusY,
                Width = e.RadiusX * 2,
                Height = e.RadiusY * 2,
            };

            RectangleF src = new RectangleF()
            {
                Left = thickness,
                Top = 0,
                Right = 0,
                Bottom = 0,
            };

            ref SpriteItem item = ref DrawInternal(null, src, bounds.TopLeft, bounds.Size, color, 0, Vector2F.Zero, material, SpriteFormat.Ellipse, 0, true);
            item.Vertex.Data.D1 = thickness / item.Vertex.Size.X; // Convert to UV coordinate system (0 - 1) range
        }
    }
}
