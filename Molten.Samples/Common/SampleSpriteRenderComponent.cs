using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;

namespace Molten.Samples.Common
{
    /// <summary>
    /// A helper component which provides a render callback for custom rendering in samples.
    /// </summary>
    public class SampleSpriteRenderComponent : SpriteRenderComponent
    {
        public SampleSpriteRenderComponent() { }

        public SampleSpriteRenderComponent(Action<SpriteBatcher> callback)
        {
            RenderCallback = callback;
        }

        protected override void OnRender(SpriteBatcher sb)
        {
            RenderCallback?.Invoke(sb);
        }

        public Action<SpriteBatcher> RenderCallback { get; set; }
    }
}
