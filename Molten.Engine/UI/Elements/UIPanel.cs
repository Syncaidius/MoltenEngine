using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UIPanel : UIElement<UIPanelData>
    {
        protected override void OnInitialize(Engine engine, UISettings settings, UITheme theme)
        {
            base.OnInitialize(engine, settings, theme);

            Properties.BorderColor = theme.DefaultColors.Border;
            Properties.BackgroundColor = theme.DefaultColors.Background;
            Properties.BorderThickness = theme.BorderThickness;
        }

        [DataMember]
        public ref Color BorderColor => ref Properties.BorderColor;

        [DataMember]
        public ref float BorderThickness => ref Properties.BorderThickness;

        [DataMember]
        public ref Color BackgroundColor => ref Properties.BackgroundColor;
    }
}
