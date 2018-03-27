using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A render task which resolves a multisampled texture into a non-multisampled one.</summary>
    internal class TextureResolve : RendererTask<TextureResolve>
    {
        public TextureBase Source;

        public TextureBase Destination;

        public override void Clear()
        {
            Source = null;
            Destination = null;
        }

        public override void Process(RendererDX11 renderer)
        {
            Source.Resolve(renderer.Device, 0, Destination, 0);
            Recycle(this);
        }
    }
}
