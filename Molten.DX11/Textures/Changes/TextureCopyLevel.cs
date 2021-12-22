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

        public uint SourceLevel;
        public uint SourceSlice;

        public uint DestinationLevel;
        public uint DestinationSlice;

        public void Process(PipeDX11 pipe, TextureBase texture)
        {

            uint srcSub = (SourceSlice * texture.MipMapCount) + SourceLevel;
            uint destSub = (DestinationSlice * Destination.MipMapCount) + DestinationLevel;

            pipe.CopyResourceRegion(texture.UnderlyingResource, srcSub, null, Destination.UnderlyingResource, destSub, Vector3UI.Zero);
            
        }
    }
}