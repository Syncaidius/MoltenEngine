using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UIPanel : UIElement
    {
        public Color BorderColor;

        public float BorderThickness;

        public Color FillColor;

        public CornerInfo CornerRadius;

        protected override void OnApplyTheme(UITheme theme, UIElementTheme elementTheme, UIStateTheme stateTheme)
        {
            base.OnApplyTheme(theme, elementTheme, stateTheme);

            BorderColor = stateTheme.BorderColor;
            BorderThickness = stateTheme.BorderThickness;
            FillColor = stateTheme.BackgroundColor;
            CornerRadius = stateTheme.CornerRadius;
        }

        internal override void Render(SpriteBatcher sb)
        {
            // TODO replace properties with this

            float radiusLimit = Math.Min(BaseData.GlobalBounds.Width, BaseData.GlobalBounds.Height) / 2;
            RoundedRectStyle style = new RoundedRectStyle()
            {
                FillColor = FillColor,
                BorderColor = BorderColor,
                BorderThickness = BorderThickness,
                CornerRadius = CornerRadius.Restrict(radiusLimit)
            };


            sb.DrawRoundedRect(BaseData.RenderBounds, 0, Vector2F.Zero, ref style);

            base.Render(sb);
        }
    }
}
