using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UIButton : UIElement<UIButtonData>
    {
        protected override void OnInitialize(Engine engine, UISettings settings, UITheme theme)
        {
            base.OnInitialize(engine, settings, theme);

            Properties.BorderColor = theme.BorderColor;
            Properties.BackgroundColor = theme.BackgroundColor;
            Properties.BorderThickness = theme.BorderThickness;
        }
    }
}
