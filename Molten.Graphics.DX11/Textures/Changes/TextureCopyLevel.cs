using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal struct TextureCopyLevel : ITextureTask
    {
        public TextureBase Destination;

        public uint SourceLevel;
        public uint SourceSlice;

        public uint DestinationLevel;
        public uint DestinationSlice;

        public unsafe void Process(PipeDX11 pipe, TextureBase texture)
        {

            uint srcSub = (SourceSlice * texture.MipMapCount) + SourceLevel;
            uint destSub = (DestinationSlice * Destination.MipMapCount) + DestinationLevel;

            pipe.CopyResourceRegion(texture.NativePtr, (uint)srcSub, null, Destination.NativePtr, (uint)destSub, Vector3UI.Zero);
            
        }
    }
}