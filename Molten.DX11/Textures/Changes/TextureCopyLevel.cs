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

        public void Process(PipeDX11 pipe, TextureBase texture)
        {

            int srcSub = (SourceSlice * texture.MipMapCount) + SourceLevel;
            int destSub = (DestinationSlice * Destination.MipMapCount) + DestinationLevel;

            int destSubID = (DestinationSlice * texture.MipMapCount) + DestinationLevel;
            pipe.CopyResourceRegion(texture.UnderlyingResource, srcSub, null, Destination.UnderlyingResource, destSub, Vector3UI.Zero);
            
        }
    }
}