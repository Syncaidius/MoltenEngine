using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UIBaseData
    {
        internal UIBaseData() { }

        /// <summary>
        /// The ID of the parent <see cref="UIBaseData"/>. Populated internally before rendering
        /// </summary>
        public UIBaseData Parent;

        /// <summary>
        /// Global position of the UI component, where 0,0 is it's origin.
        /// </summary>
        public Rectangle GlobalBounds;

        public Rectangle LocalBounds;

        public Rectangle RenderBounds;

        public Rectangle BorderBounds;

        public UISpacing Margin = new UISpacing();

        public UISpacing Padding = new UISpacing();

        public UIAnchorFlags Anchor;
    }
}
