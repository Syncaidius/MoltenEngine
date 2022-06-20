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

            public float CornerRadius;

            public void ApplyTheme(UITheme theme, UIElementTheme eTheme, UIStateTheme stateTheme)
            {
                BorderColor = stateTheme.BorderColor;
                BorderThickness = stateTheme.BorderThickness;
                BackgroundColor = stateTheme.BackgroundColor;
                CornerRadius = stateTheme.CornerRadius;
            }

            public void Render(SpriteBatcher sb, UIRenderData data)
            {
                //if (BackgroundColor.A > 0)
                //    sb.DrawRoundedRect(data.RenderBounds, BackgroundColor, 0, Vector2F.Zero, new RoundedCornerInfo(15,10,20, 5));

                //if (BorderColor.A > 0 && BorderThickness > 0)
                //    sb.DrawRectOutline(data.BorderBounds, BorderColor, BorderThickness);
            }
        }
    }
}
