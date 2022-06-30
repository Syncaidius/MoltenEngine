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
        /// Draws a circle.
        /// </summary>
        /// <param name="c">The <see cref="Circle"/> to be drawn</param>
        /// <param name="color">The color of the ellipse. Overrides the <see cref="SpriteStyle.PrimaryColor"/> of the current <see cref="SpriteStyle"/>.</param>
        /// <param name="rotation">The rotation angle, in radians.</param>  
        public void DrawCircle(ref Circle c, Color color, float rotation = 0)
        {
            ref GpuData data = ref DrawInternal(null, 
                RectangleF.Empty, c.Center, 
                new Vector2F(c.Radius * 2), 
                c.StartAngle + rotation, 
                DEFAULT_ORIGIN_CENTER, 
                null, 
                SpriteFormat.Ellipse, 
                0);

            data.Extra.D3 = c.GetAngleRange();
            data.Color = color;
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="c">The <see cref="Circle"/> to be drawn</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="origin">The origin of the circle, between 0f and 1.0f. An origin of 0.5f,0.5f would be the center of the sprite.</param>    
        public void DrawCircle(ref Circle c, Vector2F origin, float rotation = 0)
        {
            DrawCircle(ref c, origin, rotation, RectangleF.Empty);
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="c">The <see cref="Circle"/> to be drawn</param>
        /// <param name="rotation">The rotation angle, in radians.</param>    
        /// <param name="texture"></param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        public void DrawCircle(ref Circle c, float rotation = 0, ITexture2D texture = null, IMaterial material = null, float arraySlice = 0)
        {
            RectangleF source = texture != null ? new RectangleF(0, 0, texture.Width, texture.Height) : RectangleF.Empty;
            DrawCircle(ref c, DEFAULT_ORIGIN_CENTER, rotation, source, texture, material, arraySlice);
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="c">The <see cref="Circle"/> to be drawn</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="color">The color of the ellipse.</param>        
        /// <param name="source"></param>
        /// <param name="texture"></param>
        /// <param name="material"></param>
        /// <param name="arraySlice"></param>
        public void DrawCircle(ref Circle c, float rotation, RectangleF source, ITexture2D texture = null, IMaterial material = null, float arraySlice = 0)
        {
            DrawCircle(ref c, DEFAULT_ORIGIN_CENTER, rotation, source, texture, material, arraySlice);
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="c">The <see cref="Circle"/> to be drawn</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="origin">The origin of the circle, between 0f and 1.0f. An origin of 0.5f,0.5f would be the center of the sprite.</param>
        /// <param name="color">The color of the ellipse. This overrides the <see cref="SpriteStyle.PrimaryColor"/> of the current <see cref="SpriteStyle"/>.</param>        
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="arraySlice"></param>
        /// <param name="source"></param>
        /// <param name="texture"></param>
        public void DrawCircle(ref Circle c, Color color, Vector2F origin, float rotation, RectangleF source, ITexture2D texture = null, IMaterial material = null, float arraySlice = 0)
        {
            ref GpuData data = ref DrawInternal(texture, source, c.Center, new Vector2F(c.Radius * 2), c.StartAngle + rotation, origin, null, SpriteFormat.Ellipse, arraySlice);
            data.Extra.D3 = c.GetAngleRange();
            data.Color = color;
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="c">The <see cref="Circle"/> to be drawn</param>
        /// <param name="rotation">The rotation angle, in radians.</param>
        /// <param name="origin">The origin of the circle, between 0f and 1.0f. An origin of 0.5f,0.5f would be the center of the sprite.</param>
        /// <param name="material">The custom material, or null if none.</param>
        /// <param name="arraySlice"></param>
        /// <param name="source"></param>
        /// <param name="texture"></param>
        public void DrawCircle(ref Circle c, Vector2F origin, float rotation, RectangleF source, ITexture2D texture = null, IMaterial material = null, float arraySlice = 0)
        {
            ref GpuData data = ref DrawInternal(texture, source, c.Center, new Vector2F(c.Radius * 2), c.StartAngle + rotation, origin, null, SpriteFormat.Ellipse, arraySlice);
            data.Extra.D3 = c.GetAngleRange();
        }
    }
}
