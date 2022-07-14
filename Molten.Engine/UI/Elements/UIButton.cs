using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UIButton : UIElement
    {
        UIPanel _panel;
        UILabel _label;

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);

            _panel = CompoundElements.Add<UIPanel>();
            _label = CompoundElements.Add<UILabel>();

            InputRules = UIInputRuleFlags.Self | UIInputRuleFlags.Children;
        }

        protected override void OnUpdateCompoundBounds()
        {
            base.OnUpdateCompoundBounds();

            _panel.LocalBounds = new Rectangle(0, 0, BaseData.GlobalBounds.Width, BaseData.GlobalBounds.Height);
            _label.LocalBounds = _panel.LocalBounds;
        }

        public override void OnPressed(ScenePointerTracker tracker)
        {
            _panel.State = UIElementState.Pressed;
            _label.State = UIElementState.Pressed;

            base.OnPressed(tracker);
        }

        public override void OnReleased(ScenePointerTracker tracker, bool releasedOutside)
        {
            _panel.State = UIElementState.Default;
            _label.State = UIElementState.Default;

            base.OnReleased(tracker, releasedOutside);
        }

        /// <summary>
        /// The text of the current <see cref="UIButton"/>.
        /// </summary>
        public string Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        [UIThemeMember]
        public UIVerticalAlignment VerticalAlign
        {
            get => _label.VerticalAlign;
            set => _label.VerticalAlign = value;
        }

        [UIThemeMember]
        public UIHorizonalAlignment HorizontalAlign
        {
            get => _label.HorizontalAlign;
            set => _label.HorizontalAlign = value;
        }

        /// <summary>
        /// The <see cref="TextFont"/> of the current <see cref="UIButton"/>.
        /// </summary>
        public TextFont Font => _label.Font;

        /// <summary>
        /// Gets or sets the label font of the current <see cref="UIBu"/>.
        /// </summary>
        [UIThemeMember]
        public string FontName
        {
            get => _label.FontName;
            set => _label.FontName = value;
        }

        /// <summary>
        /// The corner radius values of the current <see cref="UIButton"/>. Setting them all to 0 will produce a regular rectangle.
        /// </summary>
        [UIThemeMember]
        public CornerInfo CornerRadius
        {
            get => _panel.CornerRadius;
            set => _panel.CornerRadius = value;
        }

        /// <summary>
        /// The fill/inner color of the current <see cref="UIButton"/>.
        /// </summary>
        [UIThemeMember]
        public Color FillColor
        {
            get => _panel.FillColor;
            set => _panel.FillColor = value;
        }

        /// <summary>
        /// The border color of the current <see cref="UIButton"/>.
        /// </summary>
        [UIThemeMember]
        public Color BorderColor
        {
            get => _panel.BorderColor;
            set => _panel.BorderColor = value;
        }

        /// <summary>
        /// The border line thickness of the current <see cref="UIButton"/>.
        /// </summary>
        [UIThemeMember]
        public float BorderThickness
        {
            get => _panel.BorderThickness;
            set => _panel.BorderThickness = value;
        }
    }
}
