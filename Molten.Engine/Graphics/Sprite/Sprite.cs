namespace Molten.Graphics
{
    public class Sprite
    {
        /// <summary>
        /// Gets <see cref="SpriteData"/> source used by the current <see cref="Sprite"/>. This contains texture, style and animation data.
        /// </summary>
        public SpriteData Data = new SpriteData();

        /// <summary>
        /// Gets the position of the current <see cref="Sprite"/>.
        /// </summary>
        public Vector2F Position { get; set; }

        /// <summary>
        /// Gets the rotation of the current <see cref="Sprite"/>, in radians.
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Gets the scale of the current <see cref="Sprite"/>. A scale of 1.0f is the default and original size of the sprite.
        /// </summary>
        public Vector2F Scale { get; set; } = new Vector2F(1);

        /// <summary>
        /// Gets the origin of the current <see cref="Sprite"/>.
        /// </summary>
        public Vector2F Origin { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IMaterial"/> to use when drawing the current <see cref="Sprite"/>.
        /// </summary>
        public IMaterial Material { get; set; }

        /// <summary>
        /// Gets or sets the surface array slice to which the current <see cref="Sprite"/> should be drawn.
        /// </summary>
        public uint TargetSurfaceSlice { get; set; }
    }
}
