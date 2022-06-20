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
        /// 
        /// </summary>
        /// <param name="c">The <see cref="Circle"/> to be drawn</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="origin">The origin of the circle, between 0f and 1.0f. An origin of 0.5f,0.5f would be the center of the sprite.</param>
        /// <param name="color">The color of the ellipse.</param>        
        public void DrawCircle(ref Circle c, Color color, Vector2F origin, float rotation = 0)
        {
            DrawCircle(ref c, color, origin, rotation, RectangleF.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">The <see cref="Circle"/> to be drawn</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="color">The color of the ellipse.</param>        
        /// <param name="texture"></param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        public void DrawCircle(ref Circle c, Color color, float rotation = 0, ITexture2D texture = null, IMaterial material = null, float arraySlice = 0)
        {
            RectangleF src;
            if (texture != null)
                src = new RectangleF(0, 0, texture.Width, texture.Height);
            else
                src = RectangleF.Empty;

            DrawCircle(ref c, color, new Vector2F(0.5f), rotation, src, texture, material, arraySlice);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c">The <see cref="Circle"/> to be drawn</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="color">The color of the ellipse.</param>        
        /// <param name="source"></param>
        /// <param name="texture"></param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        public void DrawCircle(ref Circle c, Color color, float rotation, RectangleF source, ITexture2D texture = null, IMaterial material = null, float arraySlice = 0)
        {
            DrawCircle(ref c, color, new Vector2F(0.5f), rotation, source, texture, material, arraySlice);
        }

        /// <summary>
        /// Draws an ellipse with the specified radius values.
        /// </summary>
        /// <param name="c">The <see cref="Circle"/> to be drawn</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="origin">The origin of the circle, between 0f and 1.0f. An origin of 0.5f,0.5f would be the center of the sprite.</param>
        /// <param name="color">The color of the ellipse.</param>        
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="arraySlice"></param>
        /// <param name="source"></param>
        /// <param name="texture"></param>
        public void DrawCircle(ref Circle c, Color color, Vector2F origin, float rotation, RectangleF source, ITexture2D texture = null, IMaterial material = null, float arraySlice = 0)
        {
            RectangleF bounds = new RectangleF()
            {
                X = c.Center.X - c.Radius,
                Y = c.Center.Y - c.Radius,
                Width = c.Radius * 2,
                Height = c.Radius * 2,
            };

            ref SpriteItem item = ref DrawInternal(texture, source, bounds.TopLeft, bounds.Size, color, rotation, origin, null, SpriteFormat.Ellipse, arraySlice, false);
            item.Vertex.Data.D2 = c.GetAngleRange();
        }

        /// <summary>
        /// Draws an ellipse with the specified radius values.
        /// </summary>
        /// <param name="c">The <see cref="Circle"/> to be drawn</param>
        /// <param name="color">The color of the ellipse.</param>        
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="thickness">The thickness of the outline.</param>
        public void DrawCircleOutline(ref Circle c, Color color, float thickness, float rotation = 0, IMaterial material = null)
        {
            DrawCircleOutline(ref c, color, DEFAULT_ORIGIN_CENTER, thickness, rotation, material);
        }

        /// <summary>
        /// Draws an ellipse with the specified radius values.
        /// </summary>
        /// <param name="c">The <see cref="Circle"/> to be drawn</param>
        /// <param name="color">The color of the ellipse.</param>        
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="thickness">The thickness of the outline.</param>
        public void DrawCircleOutline(ref Circle c, Color color, Vector2F origin, float thickness, float rotation = 0, IMaterial material = null)
        {
            RectangleF bounds = new RectangleF()
            {
                X = c.Center.X - c.Radius,
                Y = c.Center.Y - c.Radius,
                Width = c.Radius * 2,
                Height = c.Radius * 2,
            };

            ref SpriteItem item = ref DrawInternal(null, RectangleF.Empty, bounds.TopLeft, bounds.Size, color, rotation, origin, material, SpriteFormat.Ellipse, 0, true);
            item.Vertex.Data.D1 = thickness / item.Vertex.Size.X; // Convert to UV coordinate system (0 - 1) range
            item.Vertex.Data.D2 = c.GetAngleRange();
        }
    }
}
