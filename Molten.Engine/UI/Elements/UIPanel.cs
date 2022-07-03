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

            public CornerInfo CornerRadius;

            public void ApplyTheme(UITheme theme, UIElementTheme eTheme, UIStateTheme stateTheme)
            {
                BorderColor = stateTheme.BorderColor;
                BorderThickness = stateTheme.BorderThickness;
                BackgroundColor = stateTheme.BackgroundColor;
                CornerRadius = stateTheme.CornerRadius;
            }

            public void Render(SpriteBatcher sb, UIRenderData data)
            {
                // TODO replace properties with this

                float radiusLimit = Math.Min(data.GlobalBounds.Width, data.GlobalBounds.Height) / 2;
                RoundedRectStyle style = new RoundedRectStyle()
                {
                    FillColor = BackgroundColor,
                    BorderColor = BorderColor,
                    BorderThickness = BorderThickness,
                    CornerRadius = CornerRadius.Restrict(radiusLimit)
                };


                sb.DrawRoundedRect(data.RenderBounds, 0, Vector2F.Zero, ref style);
            }
        }
    }
}
