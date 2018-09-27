using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class Sprite : Renderable, ISprite
    {
        internal Sprite(GraphicsDeviceDX11 device) : base(device)
        {
        }

        internal override void Render(GraphicsPipe pipe, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data)
        {
            renderer.SpriteBatcher.Draw(Texture, Source, Position, Scale, Color, Rotation, Origin, Material, ArraySlice);
        }

        public Rectangle Source { get; set; }

        public float ArraySlice { get; set; }

        public ITexture2D Texture { get; set; }

        public Vector3F Position { get; set; }

        public Vector3F Rotation { get; set; }

        public bool IsVisible { get; set; }

        public Vector2F Scale { get; set; }

        public Vector2F Origin { get; set; }

        public IMaterial Material { get; set; }

        public Color Color { get; set; }
    }
}
