using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A render task which resolves a multisampled texture into a non-multisampled one.</summary>
    internal unsafe class TextureResolve : RendererTask<TextureResolve>
    {
        public TextureBase Source;

        public int SourceArraySlice;

        public int SourceMipLevel;

        public TextureBase Destination;

        public int DestArraySlice;

        public int DestMipLevel;

        public override void ClearForPool()
        {
            Source = null;
            Destination = null;
        }

        public override void Process(RenderService renderer)
        {
            int subSource = (Source.MipMapCount * SourceArraySlice) + SourceMipLevel;
            int subDest = (Destination.MipMapCount * DestArraySlice) + DestMipLevel;

            (renderer as RendererDX11).Device.Context->ResolveSubresource(Destination.NativePtr, (uint)subDest,
                Source.NativePtr, (uint)subSource, Source.DxgiFormat);
            Recycle(this);
        }
    }
}
