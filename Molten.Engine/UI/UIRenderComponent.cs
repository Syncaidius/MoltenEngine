using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    /// <summary>
    /// A <see cref="SceneComponent"/> used for rendering a UI system into a <see cref="Scene"/>.
    /// </summary>
    public sealed class UIRenderComponent : SpriteRenderComponent
    {
        protected override void OnDispose()
        {
            
        }

        public override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

            if (Root == null)
                return;

            // TODO build render data tree.
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            if (Root == null)
                return;

            Root.Render(sb);
        }

        /// <summary>
        /// Gets the Root UI component to be drawn.
        /// </summary>
        public UIComponent Root { get; set; }
    }
}
