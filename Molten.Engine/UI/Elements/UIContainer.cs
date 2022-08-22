using Molten.Graphics;
using System.Runtime.Serialization;

namespace Molten.UI
{
    /// <summary>
    /// A <see cref="UIElement"/> which only renders it's children. The element itself does not have anything to render.
    /// </summary>
    public class UIContainer : UIElement
    {
        UIScrollBar _vScrollBar;
        int _scrollBarWidth = 25;
        bool _scrollEnabled = true;

        protected override void OnInitialize(Engine engine, UISettings settings)
        {
            base.OnInitialize(engine, settings);

            _vScrollBar = CompoundElements.Add<UIScrollBar>();
            _vScrollBar.Set(0, 500, 20);

            _vScrollBar.ValueChanged += _vScrollBar_ValueChanged;
        }

        private void _vScrollBar_ValueChanged(UIScrollBar element)
        {
            RenderOffset = new Vector2F(0, element.Value);
        }

        protected override void OnUpdateCompoundBounds()
        {
            base.OnUpdateCompoundBounds();

            if (IsScrollingEnabled)
            {
                _vScrollBar.LocalBounds = new Rectangle()
                {
                    X = RenderBounds.Width - ScrollBarWidth,
                    Y = 0,
                    Width = _scrollBarWidth,
                    Height = RenderBounds.Height
                };
            }
        }

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

                    UpdateBounds();
                }
            }
        }
    }
}
