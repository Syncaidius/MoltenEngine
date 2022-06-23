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
        /// <param name="arraySlice">The texture array slice. This is ignored if <paramref name="texture"/> is null.</param>
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="texture">The texture, or null if none.</param>
        public void DrawEllipse(ref Ellipse e, ref SpriteStyle style, float rotation = 0, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
        {
            DrawEllipse(ref e, ref style, DEFAULT_ORIGIN_CENTER, rotation, texture, material, arraySlice);
        }

        /// <summary>
        /// Draws an ellipse with the specified radius values.
        /// </summary>
        /// <param name="e">The <see cref="Ellipse"/> to be drawn</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="arraySlice">The texture array slice. This is ignored if <paramref name="texture"/> is null.</param>
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="texture">The texture, or null if none.</param>
        /// <param name="origin">The origin of the ellipse, between 0f and 1.0f. An origin of 0.5f,0.5f would be the center of the sprite.</param>
        public void DrawEllipse(ref Ellipse e, ref SpriteStyle style, Vector2F origin, float rotation = 0, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
        {
            RectangleF bounds = new RectangleF()
            {
                X = e.Center.X,
                Y = e.Center.Y,
                Width = e.RadiusX * 2,
                Height = e.RadiusY * 2,
            };

            RectangleF source = new RectangleF(0, 0, texture.Width, texture.Height);
            ref SpriteItem item = ref DrawInternal(
                texture,
                source,
                bounds.TopLeft,
                bounds.Size,
                ref style, 
                rotation,
                origin,
                material,
                SpriteFormat.Ellipse,
                arraySlice);
            item.Vertex.Data.D1 = e.GetAngleRange();
        }

        /// <summary>
        /// Draws an ellipse with the specified radius values.
        /// </summary>
        /// <param name="e">The <see cref="Ellipse"/> to be drawn</param>
        /// <param name="color">The color of the ellipse.</param>        
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="thickness">The thickness of the outline.</param>
        public void DrawEllipseOutline(ref Ellipse e, ref SpriteStyle style, float rotation = 0, IMaterial material = null)
        {
            DrawEllipseOutline(ref e, ref style, DEFAULT_ORIGIN_CENTER, rotation, material);
        }

        /// <summary>
        /// Draws an ellipse with the specified radius values.
        /// </summary>
        /// <param name="e">The <see cref="Ellipse"/> to be drawn</param>
        /// <param name="color">The color of the ellipse.</param>        
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="thickness">The thickness of the outline.</param>
        /// <param name="origin">The origin of the ellipse, between 0f and 1.0f. An origin of 0.5f,0.5f would be the center of the sprite.</param>
        public void DrawEllipseOutline(ref Ellipse e, ref SpriteStyle style, Vector2F origin, float rotation = 0, IMaterial material = null)
        {
            RectangleF bounds = new RectangleF()
            {
                X = e.Center.X,
                Y = e.Center.Y,
                Width = e.RadiusX * 2,
                Height = e.RadiusY * 2,
            };

            ref SpriteItem item = ref DrawInternal(null, RectangleF.Empty, bounds.TopLeft, bounds.Size, ref style, rotation, origin, material, SpriteFormat.Ellipse, 0);
            item.Vertex.Data.D1 = e.GetAngleRange();
        }
    }
}
