namespace Molten.Graphics
{
    public partial class SpriteBatcher
    {
        /// <summary>Adds an untextured rectangle to the <see cref="SpriteBatcher"/>.</summary>
        /// <param name="destination">The rectangle defining the draw destination.</param>
        /// <param name="color">The color of the rectangle. Overrides the <see cref="SpriteStyle.PrimaryColor"/> of the current <see cref="SpriteStyle"/>.</param>
        /// <param name="shader">The shader to apply to the rectangle. A value of null will use the default sprite-batch material.</param>
        public void DrawRect(RectangleF destination, Color color, float rotation = 0, HlslShader shader = null, uint surfaceSlice = 0)
        {
            _rectStyle.FillColor = color;
            _rectStyle.BorderThickness.Zero();

            Draw(null, RectangleF.Empty, destination.TopLeft, destination.Size, rotation, Vector2F.Zero, ref _rectStyle, shader, 0, surfaceSlice);
        }

        /// <summary>Adds an untextured rectangle to the <see cref="SpriteBatcher"/>.</summary>
        /// <param name="destination">The rectangle defining the draw destination.</param>
        /// <param name="color">The color of the rectangle. Overrides the <see cref="SpriteStyle.PrimaryColor"/> of the current <see cref="SpriteStyle"/>.</param>
        /// <param name="shader">The shader to apply to the rectangle. A value of null will use the default sprite-batch material.</param>
        public void DrawRect(RectangleF destination, Color color, Vector2F origin, float rotation = 0, HlslShader shader = null, uint surfaceSlice = 0)
        {
            _rectStyle.FillColor = color;
            _rectStyle.BorderThickness.Zero();

            Draw(null, RectangleF.Empty, destination.TopLeft, 
                destination.Size, rotation, origin, ref _rectStyle, shader, 0, surfaceSlice);
        }

        /// <summary>Adds an untextured rectangle to the <see cref="SpriteBatcher"/>.</summary>
        /// <param name="destination">The rectangle defining the draw destination.</param>
        /// <param name="shader">The shader to apply to the rectangle. A value of null will use the default sprite-batch material.</param>
        public void DrawRect(RectangleF destination, ref RectStyle style, float rotation = 0, HlslShader shader = null, uint surfaceSlice = 0)
        {
            Draw(null, RectangleF.Empty, destination.TopLeft, destination.Size, rotation, Vector2F.Zero, ref style, shader, 0, surfaceSlice);
        }

        /// <summary>Adds an untextured rectangle to the <see cref="SpriteBatcher"/>.</summary>
        /// <param name="destination">The rectangle defining the draw destination.</param>
        /// <param name="rotation">Rotation in radians.</param>
        /// <param name="origin">The origin, as a unit value. 1.0f will set the origin to the bottom-right corner of the sprite.
        /// 0.0f will set the origin to the top-left. The origin acts as the center of the sprite.</param>
        /// <param name="shader">The shader to use when rendering the sprite.</param>
        public void DrawRect(RectangleF destination, float rotation, Vector2F origin, ref RectStyle style, HlslShader shader = null, uint surfaceSlice = 0)
        {
            Draw(null, RectangleF.Empty, destination.TopLeft, destination.Size, rotation, origin, ref style, shader, 0, surfaceSlice);
        }
    }
}
