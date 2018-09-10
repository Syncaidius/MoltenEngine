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
    public class Sprite : IRenderable2D, ISceneObject
    {
        private protected Vector2F _position;
        private protected Vector2F _scale = new Vector2F(1.0f);
        private protected Vector2F _origin = new Vector2F(0.5f);
        private protected float _rotation;

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
        public virtual Vector2F Position
        {
            get => _position;
            set => _position = value;
        }

        /// <summary>
        /// The origin of the current <see cref="Sprite"/>, as a unit vector (e.g. from 0.0f to 1.0f). The default value 0.5f (centered) on both the X and Y axis.
        /// </summary>
        public virtual Vector2F Origin
        {
            get => _origin;
            set => _origin = value;
        }

        /// <summary>
        /// The scale of the current <see cref="Sprite"/>. The default value is <see cref="Vector2F.One"/>.
        /// </summary>
        public virtual Vector2F Scale
        {
            get => _scale;
            set => _scale = value;
        }

        /// <summary>
        /// The rotation of the current <see cref="Sprite"/>, in radians.
        /// </summary>
        public virtual float Rotation
        {
            get => _rotation;
            set => _rotation = value;
        }

        Scene ISceneObject.Scene { get; set; }

        SceneLayer ISceneObject.Layer { get; set; }

        /// <summary>
        /// The color of the current <see cref="Sprite"/>. The default value is <see cref="Color.White"/>.
        /// </summary>
        public Color Color = Color.White;

        /// <summary>
        /// Called by the renderer when the sprite is to be drawn into a scene. Do not call this yourself unless you know what you are doing.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatch"/> that is performing the render operation.</param>
        public void Render(SpriteBatch sb)
        {
            sb.Draw(Texture, Position, Source, Color, Rotation, Scale, Origin);
        }
    }
}
