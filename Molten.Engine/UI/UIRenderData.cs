using Molten.Collections;
using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public class UIRenderData
    {
        internal UIRenderData() { }

        internal List<UIRenderData> Children { get; } = new List<UIRenderData>();

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

        public bool IsClipEnabled = true;
    }
}
