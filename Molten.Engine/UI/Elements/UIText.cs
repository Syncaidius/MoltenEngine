using Molten.Graphics;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Molten.UI
{
    /// <summary>
    /// A UI component dedicated to presenting text.
    /// </summary>
    public class UIText : UIElement
    {
        [JsonProperty]
        public Color Color;

        [JsonProperty]
        public string _text;

        [JsonProperty]
        private Vector2F _position;

        [IgnoreDataMember]
        public IMaterial Material;

        TextFont _font;
        Vector2F _textSize;
        UIHorizonalAlignment _hAlign;
        UIVerticalAlignment _vAlign;

        protected override void OnInitialize(Engine engine, UISettings settings, UITheme theme)
        {
            base.OnInitialize(engine, settings, theme);
            Text = Name;
            InputRules = UIInputRuleFlags.Compound | UIInputRuleFlags.Children;
        }

        protected override void OnApplyTheme(UITheme theme, UIElementTheme elementTheme, UIStateTheme stateTheme)
        {
            base.OnApplyTheme(theme, elementTheme, stateTheme);

            Color = stateTheme.TextColor;
            Font = elementTheme.Font;
        }

        internal override void Render(SpriteBatcher sb)
        {
            if (Font != null && Color.A > 0)
                sb.DrawString(Font, Text, _position, Color, Material);

            base.Render(sb);
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            if (_font == null || string.IsNullOrEmpty(Text))
                return;

            Rectangle gBounds = GlobalBounds;
            _position = (Vector2F)gBounds.TopLeft;

            switch (_hAlign)
            {
                case UIHorizonalAlignment.Center:
                    _position.X = gBounds.Center.X - (_textSize.X / 2);
                    break;

                case UIHorizonalAlignment.Right:
                    _position.X = gBounds.Right - _textSize.X;
                    break;
            }

            switch (_vAlign)
            {
                case UIVerticalAlignment.Center:
                    _position.Y = gBounds.Center.Y - (_textSize.Y / 2);
                    break;

                case UIVerticalAlignment.Bottom:
                    _position.Y = gBounds.Bottom - _textSize.Y;
                    break;
            }
        }

        /// <summary>
        /// Gets or sets the horizontal alignment.
        /// </summary>
        public UIHorizonalAlignment HorizontalAlign
        {
            get => _hAlign;
            set
            {
                if(_hAlign != value)
                {
                    _hAlign = value;
                    OnUpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets or sets the vertical alignment.
        /// </summary>
        public UIVerticalAlignment VerticalAlign
        {
            get => _vAlign;
            set
            {
                if (_vAlign != value)
                {
                    _vAlign = value;
                    OnUpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TextFont"/> of the current <see cref="UIText"/>.
        /// </summary>
        public TextFont Font
        {
            get => _font;
            set
            {
                if(_font != value)
                {
                    _font = value;
                    _textSize = _font.MeasureString(Text);
                    OnUpdateBounds();
                }
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                _textSize = _font?.MeasureString(Text) ?? new Vector2F();
                OnUpdateBounds();
            }
        }
    }
}
