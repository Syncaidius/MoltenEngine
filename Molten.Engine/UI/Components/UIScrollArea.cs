using Molten.IO;
using Molten.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    [DataContract]
    public class UIScrollArea : UICompoundComponent
    {
        UIHorizontalScrollBar _hBar;
        UIVerticalScrollBar _vBar;

        ScrollBarVisibility _hMode = ScrollBarVisibility.Auto;
        ScrollBarVisibility _vMode = ScrollBarVisibility.Auto;
        int _barThickness = 20;

        int _prevHorizontal;
        int _prevVertical;
        bool _refreshing;

        public UIScrollArea(Engine engine) : base(engine)
        {
            _enableClipping = true;

            _hBar = new UIHorizontalScrollBar(engine);
            _vBar = new UIVerticalScrollBar(engine);

            AddPart(_hBar);
            AddPart(_vBar);

            OnChildAdded += UIScrollArea_OnChildAdd;
            OnChildRemoved += UIScrollArea_OnChildRemove;

            _hBar.OnScroll += _hBar_OnScroll;
            _vBar.OnScroll += _vBar_OnScroll;
        }

        void _vBar_OnScroll(UIVerticalScrollBar component)
        {
            if (_refreshing)
                return;

            UpdateOffset();
            _prevVertical = _vBar.Value;
        }

        void _hBar_OnScroll(UIHorizontalScrollBar component)
        {
            if (_refreshing)
                return;

            UpdateOffset();
            _prevHorizontal = _hBar.Value;
        }

        void UIScrollArea_OnChildRemove(UIEventData<MouseButton> data)
        {
            RefreshBars();
        }

        void UIScrollArea_OnChildAdd(UIEventData<MouseButton> data)
        {
            ApplyCurrentOffset(data.Component);
            RefreshBars();
        }

        private void RefreshBars()
        {
            _refreshing = true;
            Rectangle bounds = new Rectangle();
            int maxX = 0;
            int maxY = 0;
            int oldvalX = _hBar.Value;
            int oldvalY = _vBar.Value;

            // Remove offsets from child components and perform scroll bounds testing
            foreach (UIComponent c in _children)
            {
                bounds = c.LocalBounds;
                bounds.X += oldvalX;
                bounds.Y += oldvalY;
                c.LocalBounds = bounds;

                if (bounds.Right > maxX)
                    maxX = bounds.Right;

                if (bounds.Bottom > maxY)
                    maxY = bounds.Bottom;
            }

            // Apply new limits to scroll bars
            int distX = Math.Max(0, maxX - _globalBounds.Width);
            int distY = Math.Max(0, maxY - _clippingBounds.Height);
            _hBar.MaxValue = distX;
            _vBar.MaxValue = distY;

            // Store the new values
            _prevHorizontal = _hBar.Value;
            _prevVertical = _vBar.Value;

            // Adjust offsets on child components to reflect changes.
            foreach (UIComponent c in _children)
                ApplyCurrentOffset(c);

            // Store current (old) visibility.
            bool hVis = _hBar.IsVisible;
            bool vVis = _vBar.IsVisible;

            // Set horizontal bar visibility.
            switch (_hMode)
            {
                case ScrollBarVisibility.Always:
                    _hBar.IsVisible = true;
                    break;

                case ScrollBarVisibility.Auto:
                    _hBar.IsVisible = (distX > 0);
                    break;

                case ScrollBarVisibility.None:
                    _hBar.IsVisible = false;
                    break;
            }

            // Set vertical bar visibility.
            switch (_vMode)
            {
                case ScrollBarVisibility.Always:
                    _vBar.IsVisible = true;
                    break;

                case ScrollBarVisibility.Auto:
                    _vBar.IsVisible = (distY > 0);
                    break;

                case ScrollBarVisibility.None:
                    _vBar.IsVisible = false;
                    break;
            }

            // Edit the clipping bounds to reflect the changes in scrollbar visibility.
            if ((hVis != _hBar.IsVisible) || (vVis != _vBar.IsVisible))
            {
                OnApplyClipPadding();
                _clippingBounds = _clipPadding.ApplyPadding(_globalBounds);

                // Update the height of the vertical scroll bar based on whether or not the horizontal one requires space.
                Rectangle vBounds = _vBar.LocalBounds;
                if (!_hBar.IsVisible)
                    vBounds.Height = _globalBounds.Height;
                else
                    vBounds.Height = _globalBounds.Height - _hBar.Height;
                _vBar.LocalBounds = vBounds;
            }

            _refreshing = false;
        }

        private void ApplyCurrentOffset(UIComponent child)
        {
            Rectangle bounds = child.LocalBounds;
            bounds.X -= _hBar.Value;
            bounds.Y -= _vBar.Value;
            child.LocalBounds = bounds;
        }

        private void UpdateOffset()
        {
            int xDif = _hBar.Value - _prevHorizontal;
            int yDif = _vBar.Value - _prevVertical;

            Rectangle bounds = new Rectangle();

            // Adjust offsets on child components to reflect changes.
            foreach (UIComponent c in _children)
            {
                bounds = c.LocalBounds;
                bounds.X -= xDif;
                bounds.Y -= yDif;
                c.LocalBounds = bounds;
            }
        }

        protected override void OnApplyClipPadding()
        {
            _clipPadding.Right = _vBar.IsVisible ? _barThickness : 0;
            _clipPadding.Bottom = _hBar.IsVisible ? _barThickness : 0;

        }

        protected override void OnUpdateBounds()
        {
            base.OnUpdateBounds();

            _hBar.LocalBounds = new Rectangle()
            {
                X = _globalBounds.X,
                Y = _globalBounds.Bottom - _barThickness,
                Width = _globalBounds.Width - _barThickness,
                Height = _barThickness,
            };

            _vBar.LocalBounds = new Rectangle()
            {
                X = _globalBounds.Right - _barThickness,
                Y = _globalBounds.Y,
                Width = _barThickness,
                Height = _globalBounds.Height - _barThickness,
            };

            RefreshBars();
        }

        [DisplayName("Scrollbar thickness")]
        [DataMember]
        public int ScrollBarThickness
        {
            get { return _barThickness; }
            set
            {
                _barThickness = value;
                OnUpdateBounds();
            }
        }

        [DataMember]
        [DisplayName("Horizontal Scroll Mode")]
        /// <summary>Gets or sets the horizontal scroll bar visibility.</summary>
        public ScrollBarVisibility HorizontalMode
        {
            get { return _hMode; }
            set
            {
                _hMode = value;
                UpdateBounds();
            }
        }

        [DataMember]
        [DisplayName("Vertical Scroll Mode")]
        /// <summary>Gets or sets the vertical scroll bar visibility.</summary>
        public ScrollBarVisibility VerticalMode
        {
            get { return _vMode; }
            set
            {
                _vMode = value;
                UpdateBounds();
            }
        }
    }
}
