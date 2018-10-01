using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class SpriteRendererDX11 : Renderable, ISpriteRenderer
    {
        internal SpriteRendererDX11(GraphicsDeviceDX11 device, Action<SpriteBatcher> callback) : base(device)
        {
            Callback = callback;
        }

        public Action<SpriteBatcher> Callback { get; set; }

        internal override void Render(GraphicsPipe pipe, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data)
        {
            Callback?.Invoke(renderer.SpriteBatcher);
            renderer.SpriteBatcher.Flush(pipe, camera, ref data.RenderTransform);
        }
    }
}
