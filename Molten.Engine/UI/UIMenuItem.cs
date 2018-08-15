using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public class UIMenuItem : UIComponent
    {
        /// <summary>
        /// The flow direction of child menu items.
        /// </summary>
        public enum ItemFlowDirection
        {
            /// <summary>
            /// Any child menu items will be listed horizontally from left to right.
            /// </summary>
            LeftToRight = 0,

            /// <summary>
            /// Any child menu items will be listed horizontally, from right to left.
            /// </summary>
            RightToLeft = 1,

            /// <summary>
            /// Any child menu items will listed vertically, from top to bottom.
            /// </summary>
            TopToBottom = 2,

            /// <summary>
            /// Any child menu items will be listed vertically, from bottom to top.
            /// </summary>
            BottomToTop = 3,
        }

        UIText _label;
        int _childSpacing = 1;
        int _iconSpacing = 10;
        int _iconSize = 16;
        int _iconMargin;
        Sprite _icon = null;

        ItemFlowDirection _flowDirection = ItemFlowDirection.TopToBottom;

        /// <summary>
        /// Creates a new instance of <see cref="UIMenuItem"/>.
        /// </summary>
        public UIMenuItem()
        {
            _label = new UIText(Engine.Current.DefaultFont, this.Name);
            UpdateIconLabel(_label);
            _label.OnTextChanged += UpdateIconLabel;
        }

        private void UpdateIconLabel(UIText obj)
        {
            _iconSize = _label.Size.Y;

            int width = 0;
            if (Parent != null && Parent is UIMenuItem parentItem)
            {
                switch (parentItem.FlowDirection)
                {
                    case ItemFlowDirection.LeftToRight:
                    case ItemFlowDirection.RightToLeft:
                        _iconMargin = _icon != null ? _iconSize + _iconSpacing : 0;
                        width = _iconMargin + Math.Max(_localBounds.Width, _label.Size.X);
                        break;

                    case ItemFlowDirection.TopToBottom:
                    case ItemFlowDirection.BottomToTop:
                        _iconMargin = _iconSize + _iconSpacing;
                        width = _iconMargin + Math.Max(_localBounds.Width, _label.Size.X);
                        break;
                }


                LocalBounds = new Rectangle()
                {
                    X = _localBounds.X,
                    Y = _localBounds.Y,
                    Width = width,
                    Height = (int)Math.Max(_localBounds.Height, _label.Size.Y),
                };

                parentItem.AlignChildItems();
            }
            else
            {
                LocalBounds = new Rectangle()
                {
                    X = _localBounds.X,
                    Y = _localBounds.Y,
                    Width = width,
                    Height = (int)Math.Max(_localBounds.Height, _label.Size.Y),
                };
            } 
        }

        /// <summary>
        /// Renders the <see cref="UIMenuItem"/>.
        /// </summary>
        /// <param name="sb"></param>
        protected override void OnRender(SpriteBatch sb)
        {
            if (_label.Color.A > 0)
                _label.Render(sb);

            base.OnRender(sb);
        }

        /// <summary>
        /// Adds a new <see cref="UIComponent"/> to the current <see cref="UIMenuItem"/>. Only <see cref="UIMenuItem"/> instances are accepted.
        /// </summary>
        /// <param name="child">The child item to add.</param>
        public override void AddChild(UIComponent child)
        {
            if (child is UIMenuItem)
            {
                base.AddChild(child);
                AlignChildItems();
            }
            else
            {
                throw new UIException(this, "UIMenuBar only accepts UIMenuItem as children.");
            }
        }

        /// <summary>
        /// Removes a child component from the current <see cref="UIMenuItem"/>.
        /// </summary>
        /// <param name="child">The child to be removed.</param>
        public override void RemoveChild(UIComponent child)
        {
            base.RemoveChild(child);
            AlignChildItems();
        }

        private void AlignChildItems()
        {
            Rectangle dest;
            UIMenuItem item;

            LockChildren(() =>
            {
                switch (_flowDirection)
                {
                    case ItemFlowDirection.LeftToRight:
                        dest = new Rectangle(0, 0, 0, Height);
                        foreach (UIComponent com in _children)
                        {
                            item = com as UIMenuItem;
                            Rectangle lBounds = item.LocalBounds;
                            dest.Width = item._iconMargin + item.Label.Size.X;

                            item.LocalBounds = dest;
                            dest.X += item.LocalBounds.Width + _childSpacing;
                        }
                        break;

                    case ItemFlowDirection.RightToLeft:

                        break;

                    case ItemFlowDirection.TopToBottom:
                        int widest = 0;
                        int destY = Height;
                        foreach (UIComponent com in _children)
                        {
                            item = com as UIMenuItem;
                            if (item.Label.Size.X > widest)
                                widest = item._iconMargin + item.Label.Size.X;
                        }

                        foreach(UIComponent com in _children)
                        {
                            item = com as UIMenuItem;
                            item.LocalBounds = new Rectangle(0, destY, widest, item.Height);
                            destY += item.Height;
                        }

                        break;

                    case ItemFlowDirection.BottomToTop:

                        break;
                }
            });
        }

        protected override void OnParentChanged()
        {
            base.OnParentChanged();
            UpdateIconLabel(_label);
        }

        protected override void UpdateBounds()
        {
            base.UpdateBounds();
            Rectangle cb = ClippingBounds;
            cb.Width -= _iconMargin;
            cb.X += _iconMargin;
            _label.Bounds = cb;

            AlignChildItems();
        }

        public override string ToString()
        {
            return $"{_label.Text} - {Name}";
        }

        /// <summary>
        /// Gets the label object of the current <see cref="UIMenuItem"/>.
        /// </summary>
        public UIText Label => _label;

        /// <summary>
        /// Gets or sets the menu item's flow direction when displaying child items.
        /// </summary>
        public ItemFlowDirection FlowDirection
        {
            get => _flowDirection;
            set
            {
                if (_flowDirection != value)
                {
                    _flowDirection = value;
                    AlignChildItems();
                }
            }
        }

        /// <summary>
        /// Gets or sets the spacing between child menu items, when displayed.
        /// </summary>
        public int ItemSpacing
        {
            get => _childSpacing;
            set
            {
                if (_childSpacing != value)
                {
                    _childSpacing = value;
                    AlignChildItems();
                }
            }
        }
    }
}
