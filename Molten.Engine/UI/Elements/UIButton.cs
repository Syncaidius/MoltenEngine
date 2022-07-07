using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    public class UIButton : UIElement
    {
        UIPanel _panel;
        UIText _label;

        protected override void OnInitialize(Engine engine, UISettings settings, UITheme theme)
        {
            base.OnInitialize(engine, settings, theme);

            _panel = CompoundElements.Add<UIPanel>();
            _label = CompoundElements.Add<UIText>();

            _label.Text = Name;
            _label.HorizontalAlign = UIHorizonalAlignment.Center;
            _label.VerticalAlign = UIVerticalAlignment.Center;
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
            base.OnPressed(tracker);

            _panel.ApplyStateTheme(UIElementState.Pressed);
            _label.ApplyStateTheme(UIElementState.Pressed);
        }

        public override void OnReleased(ScenePointerTracker tracker, bool releasedOutside)
        {
            base.OnReleased(tracker, releasedOutside);

            _panel.ApplyStateTheme(UIElementState.Default);
            _label.ApplyStateTheme(UIElementState.Default);
        }

        /// <summary>
        /// The text of the current <see cref="UIButton"/>.
        /// </summary>
        public string Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        public UIVerticalAlignment VerticalAlign
        {
            get => _label.VerticalAlign;
            set => _label.VerticalAlign = value;
        }

        public UIHorizonalAlignment HorizontalAlign
        {
            get => _label.HorizontalAlign;
            set => _label.HorizontalAlign = value;
        }

        /// <summary>
        /// The <see cref="TextFont"/> of the current <see cref="UIButton"/>.
        /// </summary>
        public TextFont Font
        {
            get => _label.Font;
            set => _label.Font = value;
        }

        /// <summary>
        /// The corner radius values of the current <see cref="UIButton"/>. Setting them all to 0 will produce a regular rectangle.
        /// </summary>
        public ref CornerInfo CornerRadius => ref _panel.CornerRadius;

        /// <summary>
        /// The fill/inner color of the current <see cref="UIButton"/>.
        /// </summary>
        public ref Color FillColor => ref _panel.FillColor;

        /// <summary>
        /// The border color of the current <see cref="UIButton"/>.
        /// </summary>
        public ref Color BorderColor => ref _panel.BorderColor;

        /// <summary>
        /// The border line thickness of the current <see cref="UIButton"/>.
        /// </summary>
        public ref float BorderThickness => ref _panel.BorderThickness;
    }
}
