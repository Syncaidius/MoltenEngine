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
                SpriteStyle style = new SpriteStyle()
                {
                    Color = BackgroundColor,
                    Color2 = BorderColor,
                    Thickness = 0,
                };

                sb.DrawRoundedRect(data.RenderBounds, ref style, CornerRadius);

                //if (BorderColor.A > 0 && BorderThickness > 0)
                //    sb.DrawRectOutline(data.BorderBounds, BorderColor, BorderThickness);
            }
        }
    }
}
