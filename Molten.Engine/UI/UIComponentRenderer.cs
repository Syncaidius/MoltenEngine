using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public abstract class UIComponentRenderer
    {
        internal UIComponentRenderer() { }

        public abstract void Render();

        /// <summary>
        /// The ID of the parent <see cref="UIComponentRenderer"/>. Populated internally before rendering
        /// </summary>
        public UIComponentRenderer ParentData;

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
