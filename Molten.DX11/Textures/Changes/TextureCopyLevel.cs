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
            if (Destination.HasFlags(TextureFlags.Dynamic))
                throw new TextureCopyException(texture, Destination, "Cannot copy to a dynamic texture via GPU. GPU cannot write to dynamic textures.");

            // Validate dimensions.
            if (Destination.Width != texture.Width ||
                Destination.Height != texture.Height ||
                Destination.Depth != texture.Depth)
                throw new TextureCopyException(texture, Destination, "The source and destination textures must have the same dimensions.");

            if (SourceLevel >= texture.MipMapCount)
                throw new TextureCopyException(texture, Destination, "The source mip-map level exceeds the total number of levels in the source texture.");

            if (SourceSlice >= texture.ArraySize)
                throw new TextureCopyException(texture, Destination, "The source array slice exceeds the total number of slices in the source texture.");

            if (DestinationLevel >= Destination.MipMapCount)
                throw new TextureCopyException(texture, Destination, "The destination mip-map level exceeds the total number of levels in the destination texture.");

            if (DestinationSlice >= Destination.ArraySize)
                throw new TextureCopyException(texture, Destination, "The destination array slice exceeds the total number of slices in the destination texture.");

            int srcSub = (SourceSlice * texture.MipMapCount) + SourceLevel;
            int destSub = (DestinationSlice * Destination.MipMapCount) + DestinationLevel;

            pipe.Context.CopySubresourceRegion(texture.UnderlyingResource, srcSub, null, Destination.UnderlyingResource, destSub);
            pipe.Profiler.CurrentFrame.CopySubresourceCount++;
        }
    }
}