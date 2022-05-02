using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UILabel : UIElement<UITextData>
    {
        UIHorizonalAlignment _hAlign;
        UIVerticalAlignment _vAlign;

        protected override void OnInitialize(Engine engine, UISettings settings, UITheme theme)
        {
            base.OnInitialize(engine, settings, theme);

            Properties.Color = theme.TextColor;
            Properties.Text = this.Name;
            theme.RequestFont(engine, LoadFont_Request);
        }

        private void LoadFont_Request(ContentRequest cr)
        {
            Properties.Font = cr.Get<SpriteFont>(0);
            OnUpdateBounds();
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            if (Properties.Font == null)
                return;

            Properties.Position = (Vector2F)LocalBounds.TopLeft;
            Vector2F textSize = Properties.Font.MeasureString(Properties.Text, 16);

            switch (_hAlign)
            {
                case UIHorizonalAlignment.Center:
                    Properties.Position.X = RenderBounds.Center.X - (textSize.X / 2);
                    break;

                case UIHorizonalAlignment.Right:
                    Properties.Position.X = RenderBounds.Right - textSize.X;
                    break;
            }

            switch (_vAlign)
            {
                case UIVerticalAlignment.Center:
                    Properties.Position.Y = RenderBounds.Center.Y - (textSize.Y / 2);
                    break;

                case UIVerticalAlignment.Bottom:
                    Properties.Position.Y = RenderBounds.Bottom - textSize.Y;
                    break;
            }
        }

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
    }
}
