using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Molten.Graphics;

namespace Molten.UI
{
    public class UITextSegment : UITextLinkable<UITextSegment>
    {
        string _text;
        SpriteFont _font;
        UITextLine _line;

        public UITextSegment()
        {
            _text = "";
        }

        /// <summary>
        /// Creates a new instance of <see cref="UITextSegment"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="font"></param>
        public UITextSegment(string text, Color color, SpriteFont font)
        {
            Text = text;
            Color = color;
            Font = font;

            CalculuateSize();
        }

        /// <summary>
        /// Invoked when the current <see cref="UITextSegment"/> should render its content.
        /// </summary>
        /// <param name="sb">The <see cref="SpriteBatcher"/> that will render the segment content.</param>
        /// <param name="owner">The parent <see cref="UITextBox"/> element.</param>
        /// <param name="bounds">The render bounds of the current <see cref="UITextSegment"/>.</param>
        public virtual void Render(SpriteBatcher sb, UITextBox owner, ref RectangleF bounds)
        {
            if (string.IsNullOrWhiteSpace(Text) || Color.A == 0)
                return;

            sb.DrawString(Font ?? owner.DefaultFont, Text, bounds.TopLeft, Color, null, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool GetFont(out SpriteFont font)
        {
            font = Font;

            if (font != null)
                return true;

            if (ParentLine != null && ParentLine.Parent != null)
            {
                font = ParentLine.Parent.DefaultFont;
                return true;
            }

            return false;
        }

        private void CalculuateSize()
        {
            if (!string.IsNullOrEmpty(_text))
            {
                if (GetFont(out SpriteFont font))
                    Size = font.MeasureString(_text);
            }
            else
            {
                Size = Vector2F.Zero;
            }
        }

        /// <summary>
        /// Measures the width of a character in <see cref="Text"/> at the given index, in pixels.
        /// </summary>
        /// <param name="index">The index of the character in <see cref="Text"/> to be measured.</param>
        /// <returns></returns>
        public float MeasureCharWidth(int index)
        {
            if (GetFont(out SpriteFont font))
                return font.MeasureCharWidth(_text[index]);

            return 0;
        }

        /// <summary>
        /// Appends the provided text to the end of the current <see cref="UITextSegment"/>.
        /// </summary>
        /// <param name="text">The text to be appended.</param>
        public void Append(string text)
        {
            _text += text;
            Vector2F appendSize = Vector2F.Zero;

            if (Font != null)
                appendSize = Font.MeasureString(text);
            else if (ParentLine != null && ParentLine.Parent != null)
                appendSize = ParentLine.Parent.DefaultFont.MeasureString(text);

            Size = new Vector2F(
                Size.X + appendSize.X, 
                MathF.Max(Size.Y, appendSize.Y)
            );
        }

        /// <summary>
        /// Inserts the provided text at the specified character index of the current <see cref="UITextSegment"/>.
        /// </summary>
        /// <param name="charIndex">The character index at which to insert text.</param>
        /// <param name="text">The text to be inserted.</param>
        public void Insert(int charIndex, string text)
        {
            _text = _text.Insert(charIndex, text);
            Vector2F appendSize = Vector2F.Zero;

            if (Font != null)
                appendSize = Font.MeasureString(text);
            else if (ParentLine != null && ParentLine.Parent != null)
                appendSize = ParentLine.Parent.DefaultFont.MeasureString(text);

            Size = new Vector2F(
                Size.X + appendSize.X,
                MathF.Max(Size.Y, appendSize.Y)
            );
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Text ?? "";
        }

        /// <summary>
        /// Gets or sets the color of the segment's <see cref="Text"/>.
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Gets the measured size of <see cref="Text"/>.
        /// <para>If <see cref="Font"/> or <see cref="Text"/> are null/empty, size will equate to <see cref="Vector2F.Zero"/>.</para>
        /// </summary>
        public Vector2F Size { get; protected set; }

        /// <summary>
        /// Gets or sets whether the current <see cref="UITextSegment"/> is selected.
        /// </summary>
        internal bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the text of the current <see cref="UITextSegment"/>. If set to null, it will be replaced with <see cref="string.Empty"/>.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                CalculuateSize();                
            }
        }

        /// <summary>
        /// Gets the length of <see cref="Text"/>.
        /// </summary>
        public int Length => _text.Length;

        /// <summary>
        /// The font of the current <see cref="UITextSegment"/>. If null, the parent <see cref="UITextBox.DefaultFont"/> will be used for measuring <see cref="Text"/>.
        /// </summary>
        public SpriteFont Font
        {
            get => _font;
            set
            {
                if(_font != value)
                {
                    _font = value;
                    CalculuateSize();
                }
            }
        }

        /// <summary>
        /// The <see cref="UITextLine"/> that the current <see cref="UITextSegment"/> is part of.
        /// </summary>
        public UITextLine ParentLine
        {
            get => _line;
            set
            {
                if(_line != value)
                {
                    _line = value;
                    CalculuateSize();
                }
            }
        }
    }
}
