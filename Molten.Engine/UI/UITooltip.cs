using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    internal class UITooltip : EngineObject
    {
        Color _mainColor;
        Color _borderColor;

        Rectangle _bounds;
        Rectangle _innerBounds;
        UILabel _text;

        bool _isValid;
        int _borderSize = 2;
        int _padding = 1;
        bool _visible;
        Vector2F _position;

        internal UITooltip()
        {
            _text = new UILabel(Engine.Current.DefaultFont, "");
            _text.OnTextChanged += _text_OnChanged;
            _text.VerticalAlignment = UIVerticalAlignment.Center;
            _text.HorizontalAlignment = UIHorizontalAlignment.Center;

            _borderColor = new Color(0, 122, 204, 255);
            _mainColor = new Color(20, 40, 60, 255);
        }

        void _text_OnChanged(UILabel text)
        {
            _isValid = !string.IsNullOrWhiteSpace(text.Text);
            if (_isValid)
                Refresh();
        }

        private void Refresh()
        {
            _innerBounds = new Rectangle()
            {
                X = (int)_position.X,
                Y = (int)_position.Y,
                Width = _text.Size.X + (_padding * 2),
                Height = _text.Size.Y + (_padding * 2),
            };

            _bounds = _innerBounds;
            _bounds.Inflate(_borderSize, _borderSize);

            _text.LocalBounds = _innerBounds;
        }

        public void Render(SpriteBatcher sb)
        {
            if (_isValid && _visible)
            {
                sb.DrawRect(_bounds, _borderColor);
                sb.DrawRect(_innerBounds, _mainColor);

                _text.Render(sb);
            }
        }

        /// <summary>Gets the rendered text instance which controls how text is displayed on the tooltip popup.</summary>
        public UILabel Text
        {
            get { return _text; }
        }

        public int BorderSize
        {
            get { return _borderSize; }
            set
            {
                _borderSize = value;
                Refresh();
            }
        }

        public Vector2F Position
        {
            get { return _position; }
            set
            {
                _position = value;
                Refresh();
            }
        }

        public int Padding
        {
            get { return _padding; }
            set
            {
                _padding = value;
                Refresh();
            }
        }

        public bool IsVisible
        {
            get { return _visible; }
            set { _visible = value; }
        }
    }
}
