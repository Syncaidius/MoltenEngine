using Molten.Graphics;

namespace Molten.UI
{
    public class UICheckBox : UIElement
    {
        public event UIElementHandler<UICheckBox> Checked;

        public event UIElementHandler<UICheckBox> Unchecked;

        public event UIElementHandler<UICheckBox> Toggled;

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
        /// The fill/inner color of the box area for the current <see cref="UICheckBox"/>.
        /// </summary>
        [UIThemeMember]
        public Color BackgroundColor = new Color(0, 109, 155, 200);

        [UIThemeMember]
        public Color CheckColor = new Color(141, 212, 242, 255);

        /// <summary>
        /// The corner radius values of the box area for the current <see cref="UICheckBox"/>. Setting them all to 0 will produce a regular rectangle.
        /// </summary>
        [UIThemeMember]
        public CornerInfo CornerRadius = new CornerInfo(3f);

        [UIThemeMember]
        public UICheckboxStyle CheckStyle = UICheckboxStyle.Fill;

        /// <summary>
        /// Gets or sets whether the current <see cref="UICheckBox"/> is checked.
        /// </summary>
        public bool IsChecked
        {
            get => _checked;
            set
            {
                if(_checked != value)
                {
                    _checked = value;
                    if (_checked)
                        Checked?.Invoke(this);
                    else
                        Unchecked?.Invoke(this);

                    Toggled?.Invoke(this);
                }
            }
        }

        UILabel _label;
        bool _checked;
        Rectangle _boxBounds;
        Rectangle _FillBounds;

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);
            BorderThickness.OnChanged += BorderThickness_OnChanged;

            _label = CompoundElements.Add<UILabel>();
            _label.Text = this.Name;
            _label.VerticalAlign = UIVerticalAlignment.Center;

            InputRules = UIInputRuleFlags.Self | UIInputRuleFlags.Children;
        }

        private void BorderThickness_OnChanged()
        {
            UpdateBounds();
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            _boxBounds = GlobalBounds;
            _boxBounds.Width = _boxBounds.Height = Math.Min(_boxBounds.Width, _boxBounds.Height);

            _label.LocalBounds = new Rectangle()
            {
                Left = _boxBounds.Width + 5,
                Top = 0,
                Right = GlobalBounds.Width,
                Bottom = GlobalBounds.Height,
            };

            _FillBounds = _boxBounds;
            _FillBounds.Inflate(-4);
        }

        protected override void OnAdjustRenderBounds(ref Rectangle renderbounds)
        {
            renderbounds.Inflate(-BorderThickness.Left, -BorderThickness.Top, -BorderThickness.Right, -BorderThickness.Bottom);
        }

        protected override bool OnPicked(Vector2F globalPos)
        {
            return _boxBounds.Contains(globalPos);
        }

        public override void OnPressed(ScenePointerTracker tracker)
        {
            base.OnPressed(tracker);
            IsChecked = !IsChecked;
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            float radiusLimit = Math.Min(GlobalBounds.Width, GlobalBounds.Height) / 2;
            RoundedRectStyle style = new RoundedRectStyle()
            {
                FillColor = BackgroundColor,
                BorderColor = BorderColor,
                BorderThickness = BorderThickness.Top,
                CornerRadius = CornerRadius.Restrict(radiusLimit)
            };

            sb.DrawRoundedRect(_boxBounds, 0, Vector2F.Zero, ref style);

            if (_checked)
            {
                switch (CheckStyle)
                {
                    case UICheckboxStyle.Tick:

                        break;

                    case UICheckboxStyle.Fill:
                        sb.DrawRoundedRect(_FillBounds, CheckColor, 0, Vector2F.Zero, CornerRadius);
                        break;
                }
            }

            base.OnRender(sb);
        }

        public string Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }
    }

    /// <summary>
    /// Represents the style of the checkbox when <see cref="UICheckBox.IsChecked"/> is true.
    /// </summary>
    public enum UICheckboxStyle
    {
        /// <summary>
        /// A tick shape.
        /// </summary>
        Tick = 0,

        /// <summary>
        /// A filled rectangle.
        /// </summary>
        Fill = 1,

        /// <summary>
        /// No check style.
        /// </summary>
        None = 2,
    }
}
