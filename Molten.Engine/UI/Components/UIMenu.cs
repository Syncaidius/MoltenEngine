using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UIMenu : UIPanel
    {
        public enum ItemFlowDirection
        {
            /// <summary>
            /// Menu items are listed horizontally.
            /// </summary>
            Horizonal = 0,

            /// <summary>
            /// Menu items arel isted vertically.
            /// </summary>
            Vertical = 1,
        }

        /// <summary>
        /// Gets or sets the flow direction of items on the menu bar.
        /// </summary>
        public ItemFlowDirection FlowDirection { get; set; }

        protected override void OnPostUpdateBounds()
        {
            base.OnPostUpdateBounds();
            Locker.Lock(() =>
            {
                switch (FlowDirection)
                {
                    case ItemFlowDirection.Horizonal:
                        int destX = ClipPadding.Left;
                        foreach (UIComponent c in _children)
                        {
                            Rectangle cBounds = c.LocalBounds;
                            cBounds.Height = ClippingBounds.Height;
                            cBounds.X = destX;
                            c.LocalBounds = cBounds;

                            destX += c.Width;
                        }
                        break;

                    case ItemFlowDirection.Vertical:

                        break;
                }
            });
        }
    }
}
