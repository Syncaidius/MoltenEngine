using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.UI
{
    public class UIContextMenu : UIComponent
    {
        bool _isOpen;

        public override void AddChild(UIComponent child)
        {
            base.AddChild(child);
            if (_isOpen)
                AlignItems();
        }

        public override void RemoveChild(UIComponent child)
        {
            base.RemoveChild(child);
            if (_isOpen)
                AlignItems();
        }

        public void Open()
        {
            _isOpen = true;

            int totalHeight = 0;
            int widest = 0;
            LockChildren(() =>
            {
                foreach (UIComponent com in _children)
                {
                    if (com.Width > widest)
                        widest = com.Width;

                    totalHeight += com.Height;
                }
            });

            if (!Margin.DockTop || !Margin.DockBottom)
                _localBounds.Height = totalHeight;

            if (!Margin.DockLeft || !Margin.DockRight)
                _localBounds.Width = widest;

            UpdateBounds();
            AlignItems();
        }

        protected override void UpdateBounds()
        {
            base.UpdateBounds();
            AlignItems();
        }

        protected override void OnRender(SpriteBatch sb)
        {
            if (!_isOpen)
                return;

            base.OnRender(sb);
        }

        private void AlignItems()
        {
            Rectangle lBounds;
            int destY = 0;
            LockChildren(() =>
            {
                foreach (UIComponent com in _children)
                {
                    if (com is UIMenuItem item)
                    {
                        lBounds = item.LocalBounds;
                        lBounds.Width = ClippingBounds.Width;
                        lBounds.X = 0;
                        lBounds.Y = destY;
                        item.LocalBounds = lBounds;
                        destY += lBounds.Height;
                    }
                }
            });
        }
    }
}
