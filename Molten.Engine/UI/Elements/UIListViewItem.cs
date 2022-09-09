using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    /// <summary>
    /// A derivative of <see cref="UIElement"/>. <see cref="UIListView"/> only accepts this class, or types rederived from it.
    /// </summary>
    public class UIListViewItem : UIElement
    {
        UILabel _label;
        RectStyle _bgStyle;
        bool _selected;

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);
            _bgStyle = new RectStyle()
            {
                BorderColor = Color.Transparent,
                BorderThickness = BorderThickness.ToThickness(),
                FillColor = new Color(0, 109, 155, 200),
            };

            _label = BaseElements.Add<UILabel>();
            BorderThickness.OnChanged += BorderThickness_OnChanged;
        }

        public override void OnReleased(ScenePointerTracker tracker, bool releasedOutside)
        {
            base.OnReleased(tracker, releasedOutside);

            if (IsSelected)
                State = UIElementState.Active;
        }

        private void BorderThickness_OnChanged()
        {
            UpdateBounds();
        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            Rectangle gb = GlobalBounds;
            _label.LocalBounds = new Rectangle(0, 0, gb.Width, gb.Height);
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            base.OnRender(sb);

            sb.DrawRect(GlobalBounds, ref _bgStyle);
        }

        /// <summary>
        /// Gets or sets the border thickness of the current <see cref="UIListViewItem"/>.
        /// </summary>
        [UIThemeMember]
        public UISpacing BorderThickness { get; } = new UISpacing(2);

        /// <summary>
        /// Gets or sets the border color of the current <see cref="UIListViewItem"/>.
        /// </summary>
        [UIThemeMember]
        public Color BorderColor
        {
            get => _bgStyle.BorderColor;
            set => _bgStyle.BorderColor = value;
        }

        /// <summary>
        /// Gets or sets the fill color of the current <see cref="UIListViewItem"/>.
        /// </summary>

        [UIThemeMember]
        public Color FillColor
        {
            get => _bgStyle.FillColor;
            set => _bgStyle.FillColor = value;
        }

        /// <summary>
        /// Gets or sets whether or not the current <see cref="UIListViewItem"/> is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _selected;
            set
            {
                if(_selected != value)
                {
                    _selected = value;
                    if (_selected)
                    {
                        if (State == UIElementState.Default)
                            State = UIElementState.Active;
                    }
                    else
                    {
                        if (State == UIElementState.Active)
                            State = UIElementState.Default;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the text of the current <see cref="UIListViewItem"/>
        /// </summary>
        public string Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }
    }
}
