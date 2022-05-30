using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UIButton : UIElement
    {
        UIPanel _panel;
        UIText _label;

        protected override void OnInitialize(Engine engine, UISettings settings, UITheme theme)
        {
            base.OnInitialize(engine, settings, theme);

            _panel = CompoundElements.Add<UIPanel>();
            _label = CompoundElements.Add<UIText>();
            _label.Text = Name;
            _label.HorizontalAlign = UIHorizonalAlignment.Center;
            _label.VerticalAlign = UIVerticalAlignment.Center;
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            _panel.LocalBounds = new Rectangle(0, 0, BaseData.GlobalBounds.Width, BaseData.GlobalBounds.Height);
            _label.LocalBounds = _panel.LocalBounds;
        }

        public string Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        public Color TextColor
        {
            get => _label.Color;
            set => _label.Color = value;
        }

        public Color BorderColor
        {
            get => _panel.BorderColor;
            set => _panel.BorderColor = value;
        }

        public Color BackgroundColor
        {
            get => _panel.BackgroundColor;
            set => _panel.BackgroundColor = value;
        }

        public UIVerticalAlignment VerticalAlign
        {
            get => _label.VerticalAlign;
            set => _label.VerticalAlign = value;
        }

        public UIHorizonalAlignment HorizontalAlign
        {
            get => _label.HorizontalAlign;
            set => _label.HorizontalAlign = value;
        }

        public TextFont Font
        {
            get => _label.Font;
            set => _label.Font = value;
        }
    }
}
