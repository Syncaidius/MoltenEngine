using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    /// <summary>
    /// A <see cref="UIElement"/> which only renders it's children. The element itself does not have anything to render.
    /// </summary>
    public class UIContainer : UIElement
    {
        UIElementLayer _overlayLayer;
        UIScrollBar _vScrollBar;
        UIScrollBar _hScrollBar;
        int _scrollBarWidth = 25;
        bool _scrollEnabled = true;

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);

            _overlayLayer = AddLayer(UIElementLayerBoundsUsage.GlobalBounds);

            _vScrollBar = _overlayLayer.Add<UIScrollBar>();
            _vScrollBar.Set(0, 500, 20);

            _hScrollBar = _overlayLayer.Add<UIScrollBar>();
            _hScrollBar.Direction = UIScrollBarDirection.Horizontal;
            _hScrollBar.Set(0, 500, 20);

            _vScrollBar.ValueChanged += ScrollBarChanged;
            _hScrollBar.ValueChanged += ScrollBarChanged;
        }

        private void ScrollBarChanged(UIScrollBar element)
        {
            RenderOffset = new Vector2F(-_hScrollBar.Value, -_vScrollBar.Value);
        }


        protected override void OnPreUpdateLayerBounds()
        {
            base.OnPreUpdateLayerBounds();

            if (IsScrollingEnabled)
            {
                _vScrollBar.LocalBounds = new Rectangle()
                {
                    X = GlobalBounds.Width - _scrollBarWidth,
                    Y = 0,
                    Width = _scrollBarWidth,
                    Height = GlobalBounds.Height - _scrollBarWidth
                };

                _hScrollBar.LocalBounds = new Rectangle()
                {
                    X = 0,
                    Y = GlobalBounds.Height - _scrollBarWidth,
                    Width = GlobalBounds.Width - _scrollBarWidth,
                    Height = _scrollBarWidth
                };
            }
        }

        /// <summary>
        /// Gets or sets the width of the horizontal and vertical scrollbars for the current <see cref="UIContainer"/>.
        /// </summary>
        [UIThemeMember]
        public int ScrollBarWidth
        {
            get => _scrollBarWidth;
            set
            {
                if(_scrollBarWidth != value)
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
                if(_scrollEnabled != value)
                {
                    _scrollEnabled = value;

                    _vScrollBar.IsVisible = value;
                    _vScrollBar.IsEnabled = value;

                    _hScrollBar.IsVisible = value;
                    _hScrollBar.IsEnabled = value;

                    UpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets the overlay layer of the current <see cref="UIContainer"/>.
        /// </summary>
        internal UIElementLayer OverlayElements => _overlayLayer;
    }
}
