using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A render task which resolves a multisampled texture into a non-multisampled one.</summary>
    internal class TextureApply : RendererTask<TextureApply>
    {
        public TextureBase Texture;

        public override void Clear()
        {
            Texture = null;
        }

        public override void Process(RendererDX11 renderer)
        {
            Texture.Apply(renderer.Device);
            Recycle(this);
        }
    }
}
