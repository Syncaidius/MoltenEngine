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

        internal override void Render(GraphicsPipe pipe, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data)
        {
            Callback?.Invoke(renderer.SpriteBatcher);
        }

        public Rectangle Source { get; set; }

        public float ArraySlice { get; set; }

        public ITexture2D Texture { get; set; }

        public Vector3F Position { get; set; }

        public Vector3F Rotation { get; set; }

        public Vector2F Scale { get; set; }

        public Vector2F Origin { get; set; }

        public IMaterial Material { get; set; }

        public Color Color { get; set; }

        public Action<SpriteBatcher> Callback { get; set; }
    }
}
