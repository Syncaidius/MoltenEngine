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
    public class UILabel : UIComponent
    {
        /// <summary>
        /// Occurs when <see cref="Text"/> or <see cref="Font"/> are changed.
        /// </summary>
        public event ObjectHandler<UILabel> OnTextChanged;

        string _text;
        SpriteFont _font;
        UIHorizontalAlignment _hAlign;
        UIVerticalAlignment _vAlign;
        Vector2F _pos;
        Vector2I _textSize;
        Color _color;

        public UILabel(SpriteFont font, string text = "")
        {
            _color = Color.White;
            _text = text;
            _font = font;
            _textSize = (Vector2I)_font.MeasureString(_text);
            _hAlign = UIHorizontalAlignment.Left;
            _vAlign = UIVerticalAlignment.Top;
            AlignText();
        }


        private void AlignText()
        {
            Rectangle cBounds = ClippingBounds;

            switch (_hAlign) {
                case UIHorizontalAlignment.Left:
                    _pos.X = cBounds.X;
                    break;

                case UIHorizontalAlignment.Center:
                    _pos.X = cBounds.Center.X - (_textSize.X / 2);
                    break;

                case UIHorizontalAlignment.Right:
                    _pos.X = cBounds.Right - _textSize.X;
                    break;
            }

            switch (_vAlign)
            {
                case UIVerticalAlignment.Top:
                    _pos.Y = cBounds.Y;
                    break;

                case UIVerticalAlignment.Center:
                    _pos.Y = cBounds.Center.Y - (_textSize.Y / 2);
                    break;

                case UIVerticalAlignment.Bottom:
                    _pos.Y = cBounds.Bottom - _textSize.Y;
                    break;
            }
        }

        protected override void OnPostUpdateBounds()
        {
            base.OnPostUpdateBounds();
            AlignText();
        }

        protected override void OnRenderUi(SpriteBatcher sb)
        {
            if(_color.A > 0)
                sb.DrawString(_font, _text, _pos, _color);

            base.OnRenderUi(sb);
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
                _textSize = (Vector2I)_font.MeasureString(_text);
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
                    _textSize = (Vector2I)_font.MeasureString(_text);
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
        /// Gets or sets the text color.
        /// </summary>
        public Color TextColor
        {
            get => _color;
            set => _color = value;
        }

        /// <summary>
        /// Gets the size of the label text based on it's current font, in pixels.
        /// </summary>
        public Vector2I Size
        {
            get => _textSize;
        }
    }
}
