using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    /// <summary>
    /// A UI component dedicated to presenting text.
    /// </summary>
    public class UIText : UIElement
    {
        [DataMember]
        public Color Color;

        [DataMember]
        public string Text;

        [DataMember]
        public Vector2F Position;

        [IgnoreDataMember]
        public IMaterial Material;

        TextFont _font;
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
                sb.DrawString(Font, Text, Position, Color, Material);

            base.Render(sb);
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            if (_font == null || string.IsNullOrEmpty(Text))
                return;

            Rectangle gBounds = GlobalBounds;
            Position = (Vector2F)gBounds.TopLeft;
            Vector2F textSize = _font.MeasureString(Text);

            switch (_hAlign)
            {
                case UIHorizonalAlignment.Center:
                    Position.X = gBounds.Center.X - (textSize.X / 2);
                    break;

                case UIHorizonalAlignment.Right:
                    Position.X = gBounds.Right - textSize.X;
                    break;
            }

            switch (_vAlign)
            {
                case UIVerticalAlignment.Center:
                    Position.Y = gBounds.Center.Y - (textSize.Y / 2);
                    break;

                case UIVerticalAlignment.Bottom:
                    Position.Y = gBounds.Bottom - textSize.Y;
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
                    OnUpdateBounds();
                }
            }
        }
    }
}
