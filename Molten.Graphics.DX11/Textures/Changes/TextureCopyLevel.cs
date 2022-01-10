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

        public unsafe void Process(PipeDX11 pipe, TextureBase texture)
        {

            int srcSub = (SourceSlice * texture.MipMapCount) + SourceLevel;
            int destSub = (DestinationSlice * Destination.MipMapCount) + DestinationLevel;

            pipe.CopyResourceRegion(texture.NativePtr, (uint)srcSub, null, Destination.NativePtr, (uint)destSub, Vector3UI.Zero);
            
        }
    }
}