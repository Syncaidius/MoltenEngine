using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UIPanel : UIElement<UIPanel.Data>
    {
        /// <summary>
        /// Container for panel render data.
        /// </summary>
        public struct Data : IUIRenderData
        {
            public Color BorderColor;

            public float BorderThickness;

            public Color BackgroundColor;

            public float BorderRadius;

            public void ApplyTheme(UITheme theme, UIElementTheme eTheme, UIStateTheme stateTheme)
            {
                BorderColor = stateTheme.BorderColor;
                BorderThickness = stateTheme.BorderThickness;
                BackgroundColor = stateTheme.BackgroundColor;
            }

            public void Render(SpriteBatcher sb, UIRenderData data)
            {
                if (BorderRadius <= 0)
                {
                    if (BackgroundColor.A > 0)
                        sb.DrawRect(data.RenderBounds, BackgroundColor);

                    if (BorderColor.A > 0 && BorderThickness > 0)
                        sb.DrawRectOutline(data.BorderBounds, BorderColor, BorderThickness);
                }
                else
                {
                    // TODO call sb.DrawRoundedRect once implemented.
                }
            }
        }
    }
}
