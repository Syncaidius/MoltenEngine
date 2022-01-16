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

        public override void ClearForPool()
        {
            Texture = null;
        }

        public override void Process(RenderService renderer)
        {
            Texture.Apply((renderer as RendererDX11).Device);
            Recycle(this);
        }
    }
}
