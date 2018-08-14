using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    /// <summary>
    /// A helper class for rendering aligned text.
    /// </summary>
    public class UIText : IRenderable2D
    {
        public event ObjectHandler<UIText> OnTextChanged;

        string _text;
        SpriteFont _font;
        UIHorizontalAlignment _hAlign;
        UIVerticalAlignment _vAlign;
        Rectangle _bounds;
        Vector2F _pos;
        Vector2F _textSize;
        Color _color;

        public UIText(SpriteFont font, string text = "")
        {
            _color = Color.White;
            _text = text;
            _font = font;
            _textSize = _font.MeasureString(_text);
            _hAlign = UIHorizontalAlignment.Left;
            _vAlign = UIVerticalAlignment.Top;
            AlignText();
        }


        private void AlignText()
        {
            switch (_hAlign) {
                case UIHorizontalAlignment.Left:
                    _pos.X = _bounds.X;
                    break;

                case UIHorizontalAlignment.Center:
                    _pos.X = _bounds.Center.X - (_textSize.X / 2);
                    break;

                case UIHorizontalAlignment.Right:
                    _pos.X = _bounds.Right - _textSize.X;
                    break;
            }

            switch (_vAlign)
            {
                case UIVerticalAlignment.Top:
                    _pos.Y = _bounds.Y;
                    break;

                case UIVerticalAlignment.Center:
                    _pos.Y = _bounds.Center.Y - (_textSize.X / 2);
                    break;

                case UIVerticalAlignment.Bottom:
                    _pos.Y = _bounds.Bottom - _textSize.Y;
                    break;
            }
        }

        public void Render(SpriteBatch sb)
        {
            if(_color.A > 0)
                sb.DrawString(_font, _text, _pos, _color);
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                _textSize = _font.MeasureString(_text);
                AlignText();
                OnTextChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Gets or sets the font used when rendering the text.
        /// </summary>
        public SpriteFont Font
        {
            get => _font;
            set
            {
                if(_font != value)
                {
                    _font = value;
                    _textSize = _font.MeasureString(_text);
                    AlignText();
                    OnTextChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal alignment of the text, within it's bounds.
        /// </summary>
        public UIHorizontalAlignment HorizontalAlignment
        {
            get => _hAlign;
            set
            {
                _hAlign = value;
                AlignText();
            }
        }

        /// <summary>
        /// Gets or sets the vertical alignment of the text, within it's bounds.
        /// </summary>
        public UIVerticalAlignment VerticalAlignment
        {
            get => _vAlign;
            set
            {
                _vAlign = value;
                AlignText();
            }
        }

        /// <summary>
        /// Gets or sets the bounds of the text.
        /// </summary>
        public Rectangle Bounds
        {
            get => _bounds;
            set
            {
                _bounds = value;
                AlignText();
            }
        }

        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        public Color Color
        {
            get => _color;
            set => _color = value;
        }

        /// <summary>
        /// Gets the size of the text based on it's current font, in pixels.
        /// </summary>
        public Vector2F Size
        {
            get => _textSize;
        }
    }
}
