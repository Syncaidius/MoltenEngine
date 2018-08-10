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

        Color _bgColor = new Color("#2d2d30");
        Color _labelColor = Color.White;
        string _label;
        int _spacing = 1;
        SpriteFont _font;
        Vector2F _labelPos;

        ItemFlowDirection _flowDirection = ItemFlowDirection.LeftToRight;

        /// <summary>
        /// Creates a new instance of <see cref="UIMenuItem"/>.
        /// </summary>
        public UIMenuItem()
        {
            _label = this.Name;
            Font = Engine.Current.DefaultFont;
        }

        /// <summary>
        /// Renders the <see cref="UIMenuItem"/>.
        /// </summary>
        /// <param name="sb"></param>
        public override void Render(SpriteBatch sb)
        {
            if(_bgColor.A > 0)
                sb.DrawRect(GlobalBounds, _bgColor);

            if (_labelColor.A > 0)
                sb.DrawString(_font, _label, _labelPos, _labelColor);

            base.Render(sb);
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

            LockChildren(() =>
            {
                switch (_flowDirection)
                {
                    case ItemFlowDirection.LeftToRight:
                        dest = new Rectangle(0,0,0, this.Height);
                        UIMenuItem item;
                        foreach (UIComponent com in _children)
                        {
                            item = com as UIMenuItem;
                            Rectangle lBounds = item.LocalBounds;
                            dest.Width = lBounds.Width;

                            item.LocalBounds = dest;
                            dest.X += item.LocalBounds.Width + _spacing;
                        }
                        break;

                    case ItemFlowDirection.RightToLeft:

                        break;

                    case ItemFlowDirection.TopToBottom:

                        break;

                    case ItemFlowDirection.BottomToTop:

                        break;
                }
            });
        }

        protected override void UpdateBounds()
        {
            Vector2F labelSize = _font.MeasureString($" {_label} ");
            Rectangle lBounds = LocalBounds;

            if(!Margin.DockLeft || !Margin.DockRight)
                lBounds.Width = (int)labelSize.X;

            if(!Margin.DockTop || !Margin.DockBottom)
                lBounds.Height = (int)(labelSize.Y * 1.1f);

            _localBounds = lBounds;
            base.UpdateBounds();

            //if (Parent != null)
            //{
            //    if (Parent is UIMenuItem parentMenuItem)
            //        parentMenuItem.AlignChildItems();
            //}

            _labelPos = LocalBounds.Center - (labelSize / 2);

            AlignChildItems();
        }

        /// <summary>
        /// Gets or sets the background color of the menu bar.
        /// </summary>
        public Color BackgroundColor
        {
            get => _bgColor;
            set => _bgColor = value;
        }

        /// <summary>
        /// Gets or sets the background color of the menu bar.
        /// </summary>
        public Color LabelColor
        {
            get => _labelColor;
            set => _labelColor = value;
        }

        /// <summary>
        /// Gets or sets the label of the current <see cref="UIMenuItem"/>.
        /// </summary>
        public string Label
        {
            get => _label;
            set
            {
                if (_label != value)
                {
                    _label = value;
                    UpdateBounds();
                }
            }
        }

        /// <summary>
        /// Gets or sets the menu item's flow direction when displaying child items.
        /// </summary>
        public ItemFlowDirection FlowDirection
        {
            get => _flowDirection;
            set
            {
                if(_flowDirection != value)
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
            get => _spacing;
            set
            {
                if(_spacing != value)
                {
                    _spacing = value;
                    AlignChildItems();
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SpriteFont"/> to use when drawing the label of the current <see cref="UIMenuItem"/>.
        /// </summary>
        public SpriteFont Font
        {
            get => _font;
            set
            {
                if(_font != value)
                {
                    _font = value;
                    UpdateBounds();
                }
            }
        }
    }
}
