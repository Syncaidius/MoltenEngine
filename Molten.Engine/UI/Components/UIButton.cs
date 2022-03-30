using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UIButton : UIComponent<UIPanel.RenderData>
    {
        protected override void OnInitialize(Engine engine, UISettings settings, UITheme theme)
        {
            base.OnInitialize(engine, settings, theme);

            Properties.BorderColor = theme.BorderColor;
            Properties.BackgroundColor = theme.BackgroundColor;
            Properties.BorderThickness = theme.BorderThickness;
        }

        /// <summary>
        /// Container for <see cref="UIPanel"/> render data.
        /// </summary>
        public struct RenderData : IUIRenderData
        {
            [DataMember]
            public UITextData Text;

            [DataMember]
            public Color BorderColor;

            [DataMember]
            public float BorderThickness;

            [DataMember]
            public Color BackgroundColor;

            public void Render(SpriteBatcher sb, UIRenderData data)
            {
                if (BackgroundColor.A > 0)
                    sb.DrawRect(data.RenderBounds, BackgroundColor);

                if (BorderColor.A > 0 && BorderThickness > 0)
                    sb.DrawRectOutline(data.BorderBounds, BorderColor, BorderThickness);

                Text.Render(sb, data);
            }
        }
    }
}
