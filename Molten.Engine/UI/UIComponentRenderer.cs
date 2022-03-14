using Molten.Graphics;
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

        internal abstract void Render(SpriteBatcher sb);

        /// <summary>
        /// The ID of the parent <see cref="UIComponentRenderer"/>. Populated internally before rendering
        /// </summary>
        public UIComponentRenderer Parent;

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

    public abstract class UIComponentRenderer<ED> : UIComponentRenderer
        where ED : struct
    {
        public ED ExtData;

        internal override void Render(SpriteBatcher sb)
        {
            // Local copy of ext data for rendering.
            ED rData = ExtData;
            OnRender(sb, ref rData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="ed">Extended data that is specific to the current <see cref="UIComponentRenderer{ED}"/></param>
        protected abstract void OnRender(SpriteBatcher sb, ref ED renderData);
    }
}
