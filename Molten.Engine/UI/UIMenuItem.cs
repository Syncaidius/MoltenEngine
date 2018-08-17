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
        const string CONTEXT_CHARACTER = "►";

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
        UIText _contextLabel;
        bool _showContextLabel;

        ItemFlowDirection _flow;
        Sprite _icon;
        int _iconSpacing = 10;
        int _shortcutMargin = 50;

        public UIMenuItem()
        {
            _flow = ItemFlowDirection.TopToBottom;

            _contextLabel = new UIText(Engine.Current.DefaultFont, CONTEXT_CHARACTER);
            _contextLabel.VerticalAlignment = UIVerticalAlignment.Center;
            _contextLabel.HorizontalAlignment = UIHorizontalAlignment.Right;
            _contextLabel.OnTextChanged += _contextLabel_OnTextChanged;

            _label = new UIText(Engine.Current.DefaultFont, this.GetType().Name);
            _label.VerticalAlignment = UIVerticalAlignment.Center;
            _label.OnTextChanged += _label_OnTextChanged;
        }

        private void _contextLabel_OnTextChanged(UIText obj)
        {
            throw new NotImplementedException();
        }

        private void _label_OnTextChanged(UIText obj)
        {
            if (Parent is UIMenuItem parentItem)
            {
                parentItem.AlignItems();
            }
            else
            {
                _localBounds.Height = _localBounds.Size.Y;
                _localBounds.Width = _label.Size.X;
                UpdateBounds();
            }
        }

        private void AlignItems()
        {
            int destX = 0;
            int destY = 0;

            switch (_flow)
            {
                case ItemFlowDirection.LeftToRight:
                    LockChildren(() =>
                    {
                        foreach(UIComponent item in _children)
                        {
                            if(item is UIMenuItem menuItem)
                            {
                                int iconSize = menuItem.Label.Size.Y;
                                int iconMargin = _icon != null ? iconSize + menuItem._iconSpacing : 0;

                                Rectangle iBounds = menuItem.LocalBounds;
                                iBounds.Width = iconMargin + menuItem.Label.Size.X;
                                iBounds.Height = Height;
                                iBounds.X = destX;
                                destX += iBounds.Width;
                                item.LocalBounds = iBounds;
                            }
                        }
                    });
                    break;

                case ItemFlowDirection.TopToBottom:
                    int widest = 0;
                    LockChildren(() =>
                    {
                        foreach (UIComponent item in _children)
                        {
                            if (item is UIMenuItem menuItem)
                            {
                                // Always include the icon margin for vertically-listed menu items.
                                int iconSize = menuItem.Label.Size.Y;
                                int iconMargin = iconSize + menuItem._iconSpacing;

                                int expectedWidth = iconMargin + menuItem.Label.Size.X + _shortcutMargin + _contextLabel.Size.X;
                                widest = Math.Max(expectedWidth, widest);
                            }
                        }

                        // Correctly set start position of children, based on the flow direction of our own parent.
                        if (Parent is UIMenuItem parentItem)
                        {
                            if (parentItem._flow == ItemFlowDirection.LeftToRight || parentItem._flow == ItemFlowDirection.RightToLeft)
                            {
                                destX = 0;
                                destY = _localBounds.Height;
                            }
                            else
                            {
                                destX = Width;
                                destY = 0;
                            }
                        }

                        // Correctly position all child items.
                        foreach (UIComponent item in _children)
                        {
                            if (item is UIMenuItem menuItem)
                            {
                                Rectangle iBounds = menuItem.LocalBounds;
                                iBounds.Height = menuItem.Label.Size.Y;
                                iBounds.Width = widest;
                                iBounds.Y = destY;
                                iBounds.X = destX;
                                destY += iBounds.Height;
                                menuItem.LocalBounds = iBounds;
                            }
                        }
                    });
                    break;
            }
        }

        protected override void UpdateBounds()
        {
            base.UpdateBounds();
            int iconSize = 0;
            int iconMargin = 0;
            Rectangle cBounds = ClippingBounds;

            // Only handle positioning of the various menu item components here.
            if(Parent != null && Parent is UIMenuItem parentItem)
            {
                switch (parentItem.FlowDirection)
                {
                    case ItemFlowDirection.LeftToRight:
                    case ItemFlowDirection.RightToLeft:
                        _showContextLabel = false;
                        iconSize = _label.Size.Y;
                        iconMargin = _icon != null ? iconSize + _iconSpacing : 0;
                        cBounds.X += iconMargin;
                        cBounds.Width -= iconMargin;
                        break;

                    case ItemFlowDirection.TopToBottom:
                    case ItemFlowDirection.BottomToTop:
                        _showContextLabel = _children.Count > 0;
                        iconSize = _label.Size.Y;
                        iconMargin = iconSize + _iconSpacing;
                        cBounds.X += iconMargin;
                        cBounds.Width -= iconMargin + _shortcutMargin + _contextLabel.Size.X;
                        _contextLabel.Bounds = new Rectangle(ClippingBounds.Right - _contextLabel.Size.X, cBounds.Y, _contextLabel.Size.X, cBounds.Height);
                        break;
                }
            }
            else
            {
                // TODO Consider fitting own icon before items if horizonal menu.
            }

            _label.Bounds = cBounds;
            AlignItems();
        }

        protected override void OnRender(SpriteBatch sb)
        {
            _label.Render(sb);
            _icon?.Render(sb);

            if (_showContextLabel)
                _contextLabel.Render(sb);

            base.OnRender(sb);
        }

        /// <summary>
        /// Gets the <see cref="UIText"/> object representing the label of the current <see cref="UIMenuItem"/>.
        /// </summary>
        public UIText Label => _label;

        /// <summary>
        /// Gets or sets the flow direction of any child items the current <see cref="UIMenuItem"/> has.
        /// </summary>
        public ItemFlowDirection FlowDirection
        {
            get => _flow;
            set
            {
                if(_flow != value)
                {
                    _flow = value;
                    AlignItems();
                }
            }
        }

        /// <summary>
        /// Gets or sets the icon of the current <see cref="UIMenuItem"/>.
        /// </summary>
        public Sprite Icon
        {
            get => _icon;
            set
            {
                if(_icon != value)
                {
                    _icon = value;
                    UpdateBounds();
                }
            }
        }
    }
}
