using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface ISpriteBatch
    {
        void DrawString(ISpriteFont font, string text, Vector2 position, Color color);

        void Draw(Rectangle destination, Color color);

        void Draw(Rectangle destination, Color color, float rotation, Vector2 origin);

        void Draw(ITexture2D texture, Rectangle destination, Color color);

        void Draw(ITexture2D texture, Rectangle destination, Color color, float rotation, Vector2 origin);

        void Draw(ITexture2D texture, Vector2 position, Color color);

        void Draw(ITexture2D texture, Vector2 position, Color color, float rotation, Vector2 origin);

        void Draw(ITexture2D texture, Vector2 position, Rectangle source, Color color, float rotation, Vector2 origin);

        void Draw(ITexture2D texture, Rectangle source, Rectangle destination, Color color, float rotation, Vector2 origin);

        void PushClip(Rectangle clipBounds);

        void PopClip();
    }
}
