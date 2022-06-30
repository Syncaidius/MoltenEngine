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
        /// <param name="color">The color of the ellipse. Overrides the <see cref="SpriteStyle.PrimaryColor"/> of the current <see cref="SpriteStyle"/>.</param>
        /// <param name="arraySlice">The texture array slice. This is ignored if <paramref name="texture"/> is null.</param>
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="texture">The texture, or null if none.</param>
        public void DrawEllipse(ref Ellipse e, Color color, float rotation = 0, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
        {
            RectangleF source = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            ref SpriteGpuData item = ref DrawInternal(
                texture,
                source,
                e.Center,
                new Vector2F(e.RadiusX * 2, e.RadiusY * 2),
                rotation,
                DEFAULT_ORIGIN_CENTER,
                material,
                SpriteFormat.Ellipse,
                arraySlice);

            item.Data.D1 = e.GetAngleRange();
            item.Color = color;
        }

        /// <summary>
        /// Draws an ellipse with the specified radius values.
        /// </summary>
        /// <param name="e">The <see cref="Ellipse"/> to be drawn</param>
        /// <param name="arraySlice">The texture array slice. This is ignored if <paramref name="texture"/> is null.</param>
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="texture">The texture, or null if none.</param>
        public void DrawEllipse(ref Ellipse e, float rotation = 0, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
        {
            DrawEllipse(ref e, DEFAULT_ORIGIN_CENTER, rotation, texture, material, arraySlice);
        }

        /// <summary>
        /// Draws an ellipse with the specified radius values.
        /// </summary>
        /// <param name="e">The <see cref="Ellipse"/> to be drawn</param>
        /// <param name="color">The color of the ellipse. Overrides the <see cref="SpriteStyle.PrimaryColor"/> of the current <see cref="SpriteStyle"/>.</param>
        /// <param name="arraySlice">The texture array slice. This is ignored if <paramref name="texture"/> is null.</param>
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="texture">The texture, or null if none.</param>
        /// <param name="origin">The origin of the ellipse, between 0f and 1.0f. An origin of 0.5f,0.5f would be the center of the sprite.</param>
        public void DrawEllipse(ref Ellipse e, Vector2F origin, float rotation = 0, ITexture2D texture = null, IMaterial material = null, uint arraySlice = 0)
        {
            RectangleF source = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            ref SpriteGpuData item = ref DrawInternal(
                texture,
                source,
                e.Center,
                new Vector2F(e.RadiusX * 2, e.RadiusY * 2),
                rotation,
                origin,
                material,
                SpriteFormat.Ellipse,
                arraySlice);
            item.Data.D1 = e.GetAngleRange();
        }
    }
}
