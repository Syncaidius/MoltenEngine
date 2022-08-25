using Molten.Graphics;

namespace Molten.UI
{
    public class UIListView : UIElement
    {
        UIScrollBar _scrollBar;
        UIPanel _panel;

        int _scrollBarWidth = 25;
        bool _scrollEnabled = true;

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);

            _panel = BaseElements.Add<UIPanel>();
            _scrollBar = BaseElements.Add<UIScrollBar>();

            _scrollBar.ValueChanged += _scrollBar_ValueChanged;
            Children.OnElementAdded += Children_OnElementAdded;
            Children.OnElementRemoved += Children_OnElementRemoved;
        }

        private void _scrollBar_ValueChanged(UIScrollBar element)
        {
            RenderOffset = new Vector2F(0, element.Value);
        }

        private void Children_OnElementRemoved(UIElement obj)
        {
            // TODO If the element is removed, was beyond our render bounds and scrolling is disabled, don't update bounds.

            OnUpdateBounds();
        }

        private void Children_OnElementAdded(UIElement obj)
        {
            // TODO If the element is added beyond our render bounds and scrolling is disabled, don't update bounds.

            OnUpdateBounds();
        }

        protected override void OnPreUpdateLayerBounds()
        {
            base.OnPreUpdateLayerBounds();

            Rectangle gb = GlobalBounds;

            if (_scrollEnabled)
            {
                int scrollingNeeded = 0;
                int panelWidth = gb.Width;

                // TODO Calculate how much scroll we need

                if (scrollingNeeded > 0)
                {
                    _scrollBar.IsVisible = true;
                    _scrollBar.IsEnabled = true;
                    panelWidth -= _scrollBarWidth;
                }


                _panel.LocalBounds = new Rectangle(0, 0, panelWidth, gb.Height);
                _scrollBar.LocalBounds = new Rectangle(_panel.LocalBounds.Right, 0, _scrollBarWidth, gb.Height);
            }
        }

        /// <summary>
        /// Gets or sets the width of the horizontal and vertical scrollbars for the current <see cref="UIListView"/>.
        /// </summary>
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
    }
}
