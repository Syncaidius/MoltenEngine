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
        /// <param name="color">The color of the rectangle. Overrides the <see cref="SpriteStyle.PrimaryColor"/> of the current <see cref="SpriteStyle"/>.</param>
        /// <param name="material">The material to apply to the rectangle. A value of null will use the default sprite-batch material.</param>
        public void DrawRect(RectangleF destination, Color color, float rotation = 0, IMaterial material = null)
        {
            ref SpriteItem item = ref DrawInternal(null, RectangleF.Empty, destination.TopLeft,
                destination.Size, rotation, Vector2F.Zero, material, SpriteFormat.Sprite, 0);

            item.Vertex.Color = color;
        }

        /// <summary>Adds an untextured rectangle to the <see cref="SpriteBatch"/>.</summary>
        /// <param name="destination">The rectangle defining the draw destination.</param>
        /// <param name="color">The color of the rectangle. Overrides the <see cref="SpriteStyle.PrimaryColor"/> of the current <see cref="SpriteStyle"/>.</param>
        /// <param name="material">The material to apply to the rectangle. A value of null will use the default sprite-batch material.</param>
        public void DrawRect(RectangleF destination, Color color, Vector2F origin, float rotation = 0, IMaterial material = null)
        {
            ref SpriteItem item = ref DrawInternal(null, RectangleF.Empty, destination.TopLeft, 
                destination.Size, rotation, origin, material, SpriteFormat.Sprite, 0);

            item.Vertex.Color = color;
        }

        /// <summary>Adds an untextured rectangle to the <see cref="SpriteBatch"/>.</summary>
        /// <param name="destination">The rectangle defining the draw destination.</param>
        /// <param name="material">The material to apply to the rectangle. A value of null will use the default sprite-batch material.</param>
        public void DrawRect(RectangleF destination, float rotation = 0, IMaterial material = null)
        {
            DrawInternal(null, RectangleF.Empty, destination.TopLeft, destination.Size, rotation, Vector2F.Zero, material, SpriteFormat.Sprite, 0);
        }

        /// <summary>Adds an untextured rectangle to the <see cref="SpriteBatch"/>.</summary>
        /// <param name="destination">The rectangle defining the draw destination.</param>
        /// <param name="rotation">Rotation in radians.</param>
        /// <param name="origin">The origin, as a unit value. 1.0f will set the origin to the bottom-right corner of the sprite.
        /// 0.0f will set the origin to the top-left. The origin acts as the center of the sprite.</param>
        /// <param name="material">The material to use when rendering the sprite.</param>
        public void DrawRect(RectangleF destination, float rotation, Vector2F origin, IMaterial material = null)
        {
            DrawInternal(null, RectangleF.Empty, destination.TopLeft, destination.Size, rotation, origin, material, SpriteFormat.Sprite, 0);
        }
    }
}
