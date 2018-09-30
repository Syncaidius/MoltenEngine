using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct TextureCopyLevel : ITextureChange
    {
        public TextureBase Destination;

        public int SourceLevel;
        public int SourceSlice;

        public int DestinationLevel;
        public int DestinationSlice;

        public void Process(GraphicsPipe pipe, TextureBase texture)
        {

            int srcSub = (SourceSlice * texture.MipMapCount) + SourceLevel;
            int destSub = (DestinationSlice * Destination.MipMapCount) + DestinationLevel;

            int destSubID = (DestinationSlice * texture.MipMapCount) + DestinationLevel;
            pipe.Context.CopySubresourceRegion(texture.UnderlyingResource, srcSub, null, Destination.UnderlyingResource, destSub);
            pipe.Profiler.Current.CopySubresourceCount++;
        }
    }
}