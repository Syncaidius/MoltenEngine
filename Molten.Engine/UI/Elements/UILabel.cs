using Molten.Graphics;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Molten.UI
{
    /// <summary>
    /// A UI component dedicated to presenting text.
    /// </summary>
    public class UILabel : UIElement
    {
        /// <summary>
        /// Invoked when either <see cref="Font"/> or <see cref="Text"/> were changed, causing a re-measurement of the text dimensions.
        /// </summary>
        public event ObjectHandler<UILabel> OnMeasurementChanged;

        [UIThemeMember]
        public Color Color;

        public string _text;

        private Vector2F _position;

        public IMaterial Material;

        TextFont _font;
        Vector2F _textSize;
        UIHorizonalAlignment _hAlign = UIHorizonalAlignment.Left;
        UIVerticalAlignment _vAlign = UIVerticalAlignment.Center;

        /// <summary>
        /// Gets the measured size of the current <see cref="Text"/> string.
        /// </summary>
        public Vector2F MeasuredSize => _textSize;

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
            _hAlign = stateTheme.HorizontalAlign;
            _vAlign = stateTheme.VerticalAlign;
            OnUpdateBounds();
        }

        protected override void OnRenderSelf(SpriteBatcher sb)
        {
            if (Font != null && Color.A > 0)
                sb.DrawString(Font, Text, _position, Color, Material);

            base.OnRenderSelf(sb);
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
        [UIThemeMember]
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
        [UIThemeMember]
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
        /// Gets or sets the <see cref="TextFont"/> of the current <see cref="UILabel"/>.
        /// </summary>
        [UIThemeMember]
        public TextFont Font
        {
            get => _font;
            set
            {
                if(_font != value)
                {
                    _font = value;
                    if (_font != null)
                    {
                        _textSize = _font.MeasureString(Text);
                        OnMeasurementChanged?.Invoke(this);
                        OnUpdateBounds();
                    }
                    else
                    {
                        _textSize = new Vector2F();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the string of text shown by the current <see cref="UILabel"/>.
        /// </summary>
        [UIThemeMember]
        public string Text
        {
            get => _text;
            set
            {
                _text = value; 
                if (_font != null)
                {
                    _textSize = _font.MeasureString(Text);
                    OnMeasurementChanged?.Invoke(this);
                    OnUpdateBounds();
                }
                else
                {
                    _textSize = new Vector2F();
                }
            }
        }
    }
}
