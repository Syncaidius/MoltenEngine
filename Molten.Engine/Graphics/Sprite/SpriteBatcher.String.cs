using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Molten.Font;
using static System.Formats.Asn1.AsnWriter;

namespace Molten.Graphics
{
    public abstract partial class SpriteBatcher
    {
        delegate void DirectionFunc(ref Vector2F charPos, ref Vector2F scale);

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
        public void DrawString(SpriteFont font, string text, Vector2F position, Color color, IMaterial material = null, uint surfaceSlice = 0)
        {
            DrawString(font, text, position, color, Vector2F.One, material, surfaceSlice);
        }

        /// <summary>Draws a string of text sprites by using a <see cref="SpriteFont"/> to source the needed data.</summary>
        /// <param name="font">The spritefont from which to retrieve font data.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The position of the text.</param>
        /// <param name="material">The material to use when rendering the string of text.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawString(SpriteFont font, string text, Vector2F position, ref TextStyle style, IMaterial material = null, uint surfaceSlice = 0)
        {
            DrawString(font, text, position, Vector2F.One, ref style, material, surfaceSlice);
        }

        /// <summary>Draws a string of text sprites by using a <see cref="SpriteFont"/> to source the needed data..</summary>
        /// <param name="font">The spritefont from which to retrieve font data.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The position of the text.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="scale">The text scale. 1.0f is equivilent to the default size. 0.5f will half the size. 2.0f will double the size.</param>
        /// <param name="material">The material to use when rendering the string of text.</param>
        public void DrawString(SpriteFont font, string text, Vector2F position, Color color, Vector2F scale, IMaterial material = null, uint surfaceSlice = 0)
        {
            if (string.IsNullOrEmpty(text))
                return;

            _textStyle.FillColor = color;
            _textStyle.DropShadowSize = 0;
            _textStyle.OutlineSize = 0;

            DrawString(font, text, position, scale, ref _textStyle, material, surfaceSlice);
        }

        /// <summary>Draws a string of text sprites by using a <see cref="SpriteFont"/> to source the needed data..</summary>
        /// <param name="font">The spritefont from which to retrieve font data.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The position of the text.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="scale">The text scale. 1.0f is equivilent to the default size. 0.5f will half the size. 2.0f will double the size.</param>
        /// <param name="material">The material to use when rendering the string of text.</param>
        public void DrawString(SpriteFont font, string text, Vector2F position, Vector2F scale, ref TextStyle style, IMaterial material = null, uint surfaceSlice = 0)
        {
            if (string.IsNullOrEmpty(text))
                return;

            // Cycle through all characters in the string and process them
            Vector2F charPos = position;
            int inc = 1;
            int start = 0;
            float totalWidth = 0;

            if (style.TextDirection == TextStyle.Direction.RightToLeft || style.TextDirection == TextStyle.Direction.BottomToTop)
            {
                inc = -1;
                start = text.Length - 1;
            }                

            for (int i = 0; i < text.Length; i++)
            {
                int c = start + (inc * i);
                SpriteFontGlyphBinding glyph = font.GetCharacter(text[c]);
                totalWidth += (glyph.AdvanceWidth * font.Scale);

                ref GpuData data = ref GetData(RangeType.MSDF, font.Manager.UnderlyingTexture, material);
                data.Rotation = 0;
                data.Position = new Vector2F()
                {
                    X = charPos.X,
                    Y = charPos.Y + ((glyph.YOffset * font.Scale) * scale.Y),
                };

                data.Array.SrcArraySlice = glyph.PageID;
                data.Array.DestSurfaceSlice = surfaceSlice;
                data.Size = (new Vector2F(glyph.Location.Width, glyph.Location.Height) * font.Scale) * scale;
                data.UV = new Vector4F(glyph.Location.Left, glyph.Location.Top, glyph.Location.Right, glyph.Location.Bottom);
                data.Color1 = style.FillColor;
                data.Color2 = style.OutlineColor;
                data.Origin = Vector2F.Zero;

                data.Extra.D1 = style.OutlineSize;
                data.Extra.D2 = style.DropShadowSize;
                data.Extra.D3 = style.DropShadowDirection.X;
                data.Extra.D4 = style.DropShadowDirection.Y;

                // Increase pos by size of char (along X)
                switch (style.TextDirection)
                {
                    case TextStyle.Direction.RightToLeft:
                    case TextStyle.Direction.LeftToRight:
                        charPos.X += (glyph.AdvanceWidth * font.Scale) * scale.X;
                        break;

                    case TextStyle.Direction.BottomToTop:
                    case TextStyle.Direction.TopToBottom:
                        charPos.Y += (glyph.AdvanceHeight * font.Scale) * scale.Y;
                        break;
                }               
            }
        }
    }
}
