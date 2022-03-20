using Silk.NET.Direct3D.Compilers;
using Buffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics
{
    internal unsafe class DxcSourceBlob
    {
        internal IDxcBlobEncoding* Ptr;

        internal Buffer BlobBuffer;
    }
}
