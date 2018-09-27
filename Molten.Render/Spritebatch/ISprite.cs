using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface ISprite : IRenderable
    {
        /// <summary>
        /// Gets or sets the source rectangle of the sprite.
        /// </summary>
        Rectangle Source { get; set; }

        /// <summary>
        /// Gets or sets the source array slice of the sprite.
        /// </summary>
        float ArraySlice { get; set; }

        ITexture2D Texture { get; set; }

        Vector3F Position { get; set; }

        Vector3F Rotation { get; set; }

        Vector2F Scale { get; set; }

        Vector2F Origin { get; set; }

        IMaterial Material { get; set; }

        Color Color { get; set; }
    }
}
