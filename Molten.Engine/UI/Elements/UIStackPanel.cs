using Molten.Graphics;

namespace Molten.UI
{
    /// <summary>
    /// A control for listing
    /// </summary>
    public class UIStackPanel : UIElement
    {
        UIScrollBar _scrollBar;
        UIPanel _panel;

        int _scrollBarWidth = 25;
        bool _scrollEnabled = true;
        UIElementFlowDirection _direction;

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);

            _panel = BaseElements.Add<UIPanel>();
            _scrollBar = BaseElements.Add<UIScrollBar>();
            _scrollBar.Increment = 10;

            _scrollBar.ValueChanged += _scrollBar_ValueChanged;
            Children.OnElementAdded += OnChildAdded;
            Children.OnElementRemoved += OnChildRemoved;

            BorderThickness.OnChanged += BorderThickness_OnChanged;
        }

        private void BorderThickness_OnChanged()
        {
            _panel.BorderThickness.Apply(BorderThickness);
        }

        private void _scrollBar_ValueChanged(UIScrollBar element)
        {
            if(_direction == UIElementFlowDirection.Vertical)
                RenderOffset = new Vector2F(0, -element.Value);
            else
                RenderOffset = new Vector2F(-element.Value, 0);
        }

        private void OnChildRemoved(UIElement obj)
        {
            // TODO If the element is removed, was beyond our render bounds and scrolling is disabled, don't update bounds.

            OnUpdateBounds();
        }

        private void OnChildAdded(UIElement obj)
        {
            // TODO If the element is added beyond our render bounds and scrolling is disabled, don't update bounds.

            OnUpdateBounds();
        }

        protected override void OnAdjustRenderBounds(ref Rectangle renderbounds)
        {
            base.OnAdjustRenderBounds(ref renderbounds);

            renderbounds.Inflate(-BorderThickness.Left, -BorderThickness.Top, -BorderThickness.Right, -BorderThickness.Bottom);
        }

        protected override void OnPreUpdateLayerBounds()
        {
            base.OnPreUpdateLayerBounds();

            Rectangle gb = GlobalBounds;

            if (_direction == UIElementFlowDirection.Vertical)
            {
                int totalHeight = 0;
                foreach (UIElement e in Children)
                {
                    Rectangle lb = e.LocalBounds;
                    lb.X = 0;
                    lb.Y = totalHeight;
                    e.LocalBounds = lb;

                    totalHeight += lb.Height;
                }

                if (_scrollEnabled)
                {
                    int scrollingNeeded = totalHeight - gb.Height;
                    int panelWidth = gb.Width;

                    if (scrollingNeeded > 0)
                    {
                        _scrollBar.IsVisible = true;
                        _scrollBar.IsEnabled = true;
                        panelWidth -= _scrollBarWidth;
                        _scrollBar.MinValue = 0;
                        _scrollBar.MaxValue = scrollingNeeded;
                    }

                    _panel.LocalBounds = new Rectangle(0, 0, panelWidth, gb.Height);
                    _scrollBar.LocalBounds = new Rectangle(_panel.LocalBounds.Right, 0, _scrollBarWidth, gb.Height);
                }
            }
            else
            {
                int totalWidth = 0;
                foreach (UIElement e in Children)
                {
                    Rectangle lb = e.LocalBounds;
                    lb.X = totalWidth;
                    lb.Y = 0;
                    e.LocalBounds = lb;

                    totalWidth += lb.Width;
                }

                if (_scrollEnabled)
                {
                    int scrollingNeeded = totalWidth - gb.Width;
                    int panelHeight = gb.Height;

                    if (scrollingNeeded > 0)
                    {
                        _scrollBar.IsVisible = true;
                        _scrollBar.IsEnabled = true;
                        panelHeight -= _scrollBarWidth;
                        _scrollBar.MinValue = 0;
                        _scrollBar.MaxValue = scrollingNeeded;
                    }

                    _panel.LocalBounds = new Rectangle(0, 0, gb.Width, panelHeight);
                    _scrollBar.LocalBounds = new Rectangle(0, _panel.LocalBounds.Bottom, gb.Width, _scrollBarWidth);
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the scrollbar for the current <see cref="UIStackPanel"/>.
        /// </summary>
        [UIThemeMember]
        public int ScrollBarWidth
        {
            get => _scrollBarWidth;
            set
            {
                if (_scrollBarWidth != value)
                {
                    _scrollBarWidth = value;
                    UpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether or not scrolling is enabled for the current <see cref="UIContainer"/>.
        /// </summary>
        [UIThemeMember]
        public bool IsScrollingEnabled
        {
            get => _scrollEnabled;
            set
            {
                if (_scrollEnabled != value)
                {
                    _scrollEnabled = value;
                    UpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets or sets the border thickness of the current <see cref="UIWindow"/>.
        /// </summary>
        [UIThemeMember]
        public UISpacing BorderThickness { get; } = new UISpacing(2);

        /// <summary>
        /// Gets or sets the flow direction of the stack panel.
        /// </summary>
        [UIThemeMember]
        public UIElementFlowDirection Direction
        {
            get => _direction;
            set
            {
                if(_direction != value)
                {
                    _direction = value;
                    _scrollBar.Direction = _direction;
                    OnUpdateBounds();
                }
            }
        }
    }
}
