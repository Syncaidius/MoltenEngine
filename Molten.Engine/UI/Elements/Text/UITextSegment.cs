using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public class UITextSegment
    {
        string _text;
        SpriteFont _font;
        UITextLine _line;

        public UITextSegment()
        {
            _text = "";
        }

        public UITextSegment(string text, Color color, SpriteFont font)
        {
            Text = text;
            Color = color;
            Font = font;

            Measure();
        }

        public virtual void Render(SpriteBatcher sb, UITextBox owner, ref RectangleF bounds)
        {
            if (string.IsNullOrWhiteSpace(Text) || Color.A == 0)
                return;

            sb.DrawString(Font ?? owner.DefaultFont, Text, bounds.TopLeft, Color, null, 0);
        }

        private void Measure()
        {
            if (!string.IsNullOrEmpty(_text))
            {
                if (Font != null)
                    Size = Font.MeasureString(_text);
                else if (ParentLine != null && ParentLine.Parent != null)
                    Size = ParentLine.Parent.DefaultFont.MeasureString(_text);
            }
            else
            {
                Size = Vector2F.Zero;
            }
        }

        public void Append(string newText)
        {
            _text += newText;
            Vector2F appendSize = Vector2F.Zero;

            if (Font != null)
                appendSize = Font.MeasureString(newText);
            else if (ParentLine != null && ParentLine.Parent != null)
                appendSize = ParentLine.Parent.DefaultFont.MeasureString(newText);

            Size = new Vector2F(
                Size.X + appendSize.X, 
                MathF.Max(Size.Y, appendSize.Y)
            );
        }

        public void Insert(int charIndex, string newText)
        {
            _text = _text.Insert(charIndex, newText);
            Vector2F appendSize = Vector2F.Zero;

            if (Font != null)
                appendSize = Font.MeasureString(newText);
            else if (ParentLine != null && ParentLine.Parent != null)
                appendSize = ParentLine.Parent.DefaultFont.MeasureString(newText);

            Size = new Vector2F(
                Size.X + appendSize.X,
                MathF.Max(Size.Y, appendSize.Y)
            );
        }

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
        /// Gets the previous <see cref="UITextSegment"/>, or null if none.
        /// </summary>
        public UITextSegment Previous { get; internal set; }

        /// <summary>
        /// Gets the next <see cref="UITextSegment"/>, or null if none.
        /// </summary>
        public UITextSegment Next { get; internal set; }

        internal bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the text of the current <see cref="UITextSegment"/>.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                Measure();                
            }
        }

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
                    Measure();
                }
            }
        }

        public UITextLine ParentLine
        {
            get => _line;
            set
            {
                if(_line != value)
                {
                    _line = value;
                    Measure();
                }
            }
        }
    }
}
