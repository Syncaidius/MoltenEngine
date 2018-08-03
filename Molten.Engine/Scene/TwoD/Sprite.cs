using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>
    /// An basic sprite class, incapable of animation or parenting. Useful for drawing static objects with very little overhead. It is also the base class for <see cref="AnimatedSprite"/>.
    /// </summary>
    public class Sprite : IRenderable2D
    {
        /// <summary>
        /// The sprite's source texture.
        /// </summary>
        public ITexture2D Texture;

        /// <summary>
        /// The source rectangle. This tells the renderer which part of a texture makes up the current sprite.
        /// </summary>
        public Rectangle Source;

        /// <summary>
        /// The screen position of the current <see cref="Sprite"/>.
        /// </summary>
        public Vector2F Position;

        /// <summary>
        /// The rotation of the current <see cref="Sprite"/>.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// The origin of the current <see cref="Sprite"/>, as a unit vector (e.g. from 0.0f to 1.0f). The default value is <see cref="Vector2F.Zero"/>.
        /// </summary>
        public Vector2F Origin = Vector2F.Zero;

        /// <summary>
        /// The color of the current <see cref="Sprite"/>. The default value is <see cref="Color.White"/>.
        /// </summary>
        public Color Color = Color.White;

        /// <summary>
        /// The scale of the current <see cref="Sprite"/>. The default value is <see cref="Vector2F.One"/>.
        /// </summary>
        public Vector2F Scale = Vector2F.One;

        /// <summary>
        /// Called by the renderer when the sprite is to be drawn into a scene. Do not call this yourself unless you know what you are doing.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> that is performing the render operation.</param>
        void IRenderable2D.Render(SpriteBatch sb)
        {
            sb.Draw(Texture, Position, Source, Color, Rotation, Scale, Origin);
        }
    }
}
