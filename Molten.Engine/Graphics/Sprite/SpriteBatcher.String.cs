using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract partial class SpriteBatcher
    {
        /// <summary>Draws a string of text sprites by using a <see cref="SpriteFont"/> to source the needed data.</summary>
        /// <param name="font">The spritefont from which to retrieve font data.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The position of the text.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="material">The material to use when rendering the string of text.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(TextFont font, string text, Vector2F position, Color color, IMaterial material = null)
        {
            DrawString(font, text, position, color, Vector2F.One, material);
        }

        /// <summary>Draws a string of text sprites by using a <see cref="SpriteFont"/> to source the needed data.</summary>
        /// <param name="font">The spritefont from which to retrieve font data.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The position of the text.</param>
        /// <param name="material">The material to use when rendering the string of text.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(TextFont font, string text, Vector2F position, IMaterial material = null)
        {
            DrawString(font, text, position, Vector2F.One, material);
        }

        /// <summary>Draws a string of text sprites by using a <see cref="SpriteFont"/> to source the needed data..</summary>
        /// <param name="font">The spritefont from which to retrieve font data.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The position of the text.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="scale">The text scale. 1.0f is equivilent to the default size. 0.5f will half the size. 2.0f will double the size.</param>
        /// <param name="material">The material to use when rendering the string of text.</param>
        public void DrawString(TextFont font, string text, Vector2F position, Vector2F scale, IMaterial material = null)
        {
            if (string.IsNullOrEmpty(text))
                return;

            // Cycle through all characters in the string and process them
            Vector2F charPos = position;
            for (int i = 0; i < text.Length; i++)
            {
                TextFontSource.CachedGlyph cache = font.Source.GetCharGlyph(text[i]);

                ref SpriteItem item = ref GetItem();
                item.Texture = font.Source.UnderlyingTexture;
                item.Material = material;
                item.Format = SpriteFormat.MSDF;

                item.Vertex.Position = new Vector2F(charPos.X, charPos.Y + ((cache.YOffset * font.Scale) * scale.Y));
                item.Vertex.Rotation = 0; // TODO 2D text rotation.
                item.Vertex.ArraySlice = 0; // TODO SpriteFont array slice support.
                item.Vertex.Size = (new Vector2F(cache.Location.Width, cache.Location.Height) * font.Scale) * scale;
                item.Vertex.UV = new Vector4F(cache.Location.Left, cache.Location.Top, cache.Location.Right, cache.Location.Bottom);
                item.Vertex.Color = _style.PrimaryColor;
                item.Vertex.Color2 = _style.SecondaryColor;
                item.Vertex.Data.Thickness = new Vector2F(_style.Thickness);
                item.Vertex.Origin = Vector2F.Zero;

                // Increase pos by size of char (along X)
                charPos.X += (cache.AdvanceWidth * font.Scale) * scale.X;
            }
        }

        /// <summary>Draws a string of text sprites by using a <see cref="SpriteFont"/> to source the needed data..</summary>
        /// <param name="font">The spritefont from which to retrieve font data.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The position of the text.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="scale">The text scale. 1.0f is equivilent to the default size. 0.5f will half the size. 2.0f will double the size.</param>
        /// <param name="material">The material to use when rendering the string of text.</param>
        public void DrawString(TextFont font, string text, Vector2F position, Color color, Vector2F scale, IMaterial material = null)
        {
            if (string.IsNullOrEmpty(text))
                return;

            // Cycle through all characters in the string and process them
            Vector2F charPos = position;
            for (int i = 0; i < text.Length; i++)
            {
                TextFontSource.CachedGlyph cache = font.Source.GetCharGlyph(text[i]);

                ref SpriteItem item = ref GetItem();
                item.Texture = font.Source.UnderlyingTexture;
                item.Material = material;
                item.Format = SpriteFormat.MSDF;

                item.Vertex.Position = new Vector2F(charPos.X, charPos.Y + ((cache.YOffset * font.Scale) * scale.Y));
                item.Vertex.Rotation = 0; // TODO 2D text rotation.
                item.Vertex.ArraySlice = 0; // TODO SpriteFont array slice support.
                item.Vertex.Size = (new Vector2F(cache.Location.Width, cache.Location.Height) * font.Scale) * scale;
                item.Vertex.UV = new Vector4F(cache.Location.Left, cache.Location.Top, cache.Location.Right, cache.Location.Bottom);
                item.Vertex.Color = color;
                item.Vertex.Origin = Vector2F.Zero;

                // Increase pos by size of char (along X)
                charPos.X += (cache.AdvanceWidth * font.Scale) * scale.X;
            }
        }
    }
}
