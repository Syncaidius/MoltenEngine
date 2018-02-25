using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface ISpriteBatch
    {
        void DrawString(ISpriteFont font, string text, Vector2 position, Color color, IMaterial material = null);

        void Draw(Rectangle destination, Color color, IMaterial material = null);

        void Draw(Rectangle destination, Color color, float rotation, Vector2 origin, IMaterial material = null);

        void Draw(ITexture2D texture, Rectangle destination, Color color, IMaterial material = null);

        void Draw(ITexture2D texture, Rectangle destination, Color color, float rotation, Vector2 origin, IMaterial material = null);

        void Draw(ITexture2D texture, Vector2 position, Color color, IMaterial material = null);

        void Draw(ITexture2D texture, Vector2 position, Color color, float rotation, Vector2 origin, IMaterial material = null);

        void Draw(ITexture2D texture, Vector2 position, Rectangle source, Color color, float rotation, Vector2 scale, Vector2 origin, IMaterial material = null);

        void Draw(ITexture2D texture, Rectangle source, Rectangle destination, Color color, float rotation, Vector2 origin, IMaterial material);

        /// <summary>
        /// Adds the contents of the specified <see cref="SpriteBatchCache"/> to the current <see cref="ISpriteBatch"/>.
        /// </summary>
        /// <param name="cache">The cache.</param>
        void Draw(SpriteBatchCache cache);

        void PushClip(Rectangle clipBounds);

        void PopClip();

        /// <summary>Gets the current clip depth. This increases and decreases with calls to <see cref="PushClip(Rectangle)"/> and <see cref="PopClip"/></summary>
        int ClipDepth { get; }
    }
}
