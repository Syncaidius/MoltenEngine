using Molten.Graphics;

namespace Molten.UI
{
    public class UIPanel : UIElement
    {
        /// <summary>
        /// The border color
        /// </summary>
        [UIThemeMember]
        public Color BorderColor = new Color(52, 189, 235, 255);

        /// <summary>
        /// The border line thickness
        /// </summary>
        [UIThemeMember]
        public UISpacing BorderThickness { get; } = new UISpacing(2);

        /// <summary>
        /// The fill/inner color of the current <see cref="UIPanel"/>.
        /// </summary>
        [UIThemeMember]
        public Color FillColor = new Color(0, 109, 155, 200);

        /// <summary>
        /// The corner radius values of the current <see cref="UIPanel"/>. Setting them all to 0 will produce a regular rectangle.
        /// </summary>
        [UIThemeMember]
        public CornerInfo CornerRadius = new CornerInfo(8f);

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);
            BorderThickness.OnChanged += BorderThickness_OnChanged;

            InputRules = UIInputRuleFlags.Compound | UIInputRuleFlags.Compound;
        }

        private void BorderThickness_OnChanged()
        {
            UpdateBounds();
        }

        protected override void OnAdjustRenderBounds(ref Rectangle renderbounds)
        {
            renderbounds.Inflate(-BorderThickness.Left, -BorderThickness.Top, -BorderThickness.Right, -BorderThickness.Bottom);
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            float radiusLimit = Math.Min(GlobalBounds.Width, GlobalBounds.Height) / 2;
            RoundedRectStyle style = new RoundedRectStyle()
            {
                FillColor = FillColor,
                BorderColor = BorderColor,
                BorderThickness = BorderThickness.Top,
                CornerRadius = CornerRadius.Restrict(radiusLimit)
            };

            sb.DrawRoundedRect(GlobalBounds, 0, Vector2F.Zero, ref style);

            base.OnRender(sb);
        }
    }
}
