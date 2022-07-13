using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UIPanel : UIElement
    {
        /// <summary>
        /// The border color
        /// </summary>
        [UIThemeMember]
        public Color BorderColor;

        /// <summary>
        /// The border line thickness
        /// </summary>
        [UIThemeMember]
        public float BorderThickness;

        /// <summary>
        /// The fill/inner color of the current <see cref="UIPanel"/>.
        /// </summary>
        [UIThemeMember]
        public Color FillColor;

        /// <summary>
        /// The corner radius values of the current <see cref="UIPanel"/>. Setting them all to 0 will produce a regular rectangle.
        /// </summary>
        [UIThemeMember]
        public CornerInfo CornerRadius;

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);

            InputRules = UIInputRuleFlags.Compound | UIInputRuleFlags.Compound;
        }

        protected override void OnRenderSelf(SpriteBatcher sb)
        {
            float radiusLimit = Math.Min(BaseData.GlobalBounds.Width, BaseData.GlobalBounds.Height) / 2;
            RoundedRectStyle style = new RoundedRectStyle()
            {
                FillColor = FillColor,
                BorderColor = BorderColor,
                BorderThickness = BorderThickness,
                CornerRadius = CornerRadius.Restrict(radiusLimit)
            };


            sb.DrawRoundedRect(BaseData.RenderBounds, 0, Vector2F.Zero, ref style);

            base.OnRenderSelf(sb);
        }
    }
}
