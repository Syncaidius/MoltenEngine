using Molten.Graphics;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Molten.UI
{
    /// <summary>
    /// A UI label element.
    /// </summary>
    public class UILabel : UIElement
    {
        /// <summary>
        /// Invoked when either <see cref="Font"/> or <see cref="Text"/> were changed, causing a re-measurement of the text dimensions.
        /// </summary>
        public event ObjectHandler<UILabel> OnMeasurementChanged;

        public string _text;
        private Vector2F _position;
        public IMaterial Material;

        string _fontName;
        SpriteFont _font;
        Vector2F _textSize;
        UIHorizonalAlignment _hAlign = UIHorizonalAlignment.Left;
        UIVerticalAlignment _vAlign = UIVerticalAlignment.Top;

        /// <summary>
        /// Gets the measured size of the current <see cref="Text"/> string.
        /// </summary>
        public Vector2F MeasuredSize => _textSize;

        public UILabel()
        {
            _text = Name;
        }

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);
            FontName = settings.DefaultFontName;
            InputRules = UIInputRuleFlags.Compound | UIInputRuleFlags.Children;
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            if (_font != null && Color.A > 0)
                sb.DrawString(_font, _text, _position, Color, Material);

            base.OnRender(sb);
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
        /// Gets the <see cref="TextFont"/> of the current <see cref="UILabel"/>.
        /// </summary>
        public SpriteFont Font
        {
            get => _font;
            private set
            {
                if(_font != value)
                {
                    // Unbind previous font
                    if(_font != null)
                        _font.OnSizeChanged -= RecalculateMeasurements;

                    // Bind new font, if any.
                    _font = value;
                    if (_font != null)
                    {
                        RecalculateMeasurements(_font);
                        _font.OnSizeChanged += RecalculateMeasurements;                   
                    }
                    else
                    {
                        _textSize = new Vector2F();
                    }
                }
            }
        }

        private void RecalculateMeasurements(SpriteFont font)
        {
            if (_text == null)
                return;

            _textSize = font.MeasureString(_text);
            OnMeasurementChanged?.Invoke(this);
            OnUpdateBounds();
        }

        /// <summary>
        /// Gets or sets the text color of the current <see cref="UILabel"/>.
        /// </summary>
        [UIThemeMember]
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets the name of the font for the current <see cref="UILabel"/>. This will attempt to load/retrieve and populate <see cref="Font"/>.
        /// </summary>
        [UIThemeMember]
        public string FontName
        {
            get => _fontName;
            set
            {
                value = (value ?? string.Empty).ToLower();
                if(_fontName != value)
                {
                    _fontName = value;
                    if (!string.IsNullOrWhiteSpace(_fontName))
                    {
                        Engine.Content.LoadFont(_fontName, (font, isReload) =>
                        {
                            Font = font;
                        },
                        new SpriteFontParameters()
                        {
                            FontSize = 16,
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the string of text shown by the current <see cref="UILabel"/>.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value; 
                if (_font != null)
                    RecalculateMeasurements(_font);
                else
                    _textSize = new Vector2F();
            }
        }
    }
}
