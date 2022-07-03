﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract partial class SpriteBatcher
    {
        /// <summary>
        /// Placeholder text style.
        /// </summary>
        TextStyle _textStyle; 

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
        public void DrawString(TextFont font, string text, Vector2F position, ref TextStyle style, IMaterial material = null)
        {
            DrawString(font, text, position, Vector2F.One, ref style, material);
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

            _textStyle.FillColor = color;
            _textStyle.DropShadowSize = 0;
            _textStyle.OutlineSize = 0;

            DrawString(font, text, position, scale, ref _textStyle, material);
        }

        /// <summary>Draws a string of text sprites by using a <see cref="SpriteFont"/> to source the needed data..</summary>
        /// <param name="font">The spritefont from which to retrieve font data.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The position of the text.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="scale">The text scale. 1.0f is equivilent to the default size. 0.5f will half the size. 2.0f will double the size.</param>
        /// <param name="material">The material to use when rendering the string of text.</param>
        public void DrawString(TextFont font, string text, Vector2F position, Vector2F scale, ref TextStyle style, IMaterial material = null)
        {
            if (string.IsNullOrEmpty(text))
                return;

            // Cycle through all characters in the string and process them
            Vector2F charPos = position;
            for (int i = 0; i < text.Length; i++)
            {
                TextFontSource.CachedGlyph cache = font.Source.GetCharGlyph(text[i]);

                uint id = GetItemID();
                ref SpriteItem item = ref Sprites[id];
                item.Texture = font.Source.UnderlyingTexture;
                item.Material = material;
                item.Type = ItemType.MSDF;

                ref GpuData data = ref Data[id];
                data.Position = new Vector2F(charPos.X, charPos.Y + ((cache.YOffset * font.Scale) * scale.Y));
                data.Rotation = 0; // TODO 2D text rotation.
                data.ArraySlice = 0; // TODO SpriteFont array slice support.
                data.Size = (new Vector2F(cache.Location.Width, cache.Location.Height) * font.Scale) * scale;
                data.UV = new Vector4F(cache.Location.Left, cache.Location.Top, cache.Location.Right, cache.Location.Bottom);
                data.Color1 = style.FillColor;
                data.Color2 = style.OutlineColor;
                data.Origin = Vector2F.Zero;

                data.Extra.D1 = style.OutlineSize;
                data.Extra.D2 = style.DropShadowSize;
                data.Extra.D3 = style.DropShadowDirection.X;
                data.Extra.D4 = style.DropShadowDirection.Y;

                // Increase pos by size of char (along X)
                charPos.X += (cache.AdvanceWidth * font.Scale) * scale.X;
            }
        }
    }
}
