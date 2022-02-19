using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buffer = Silk.NET.Direct3D.Compilers.Buffer;

namespace Molten.Graphics
{
    internal unsafe class DxcSourceBlob
    {
        internal IDxcBlobEncoding* Ptr;

        internal Buffer BlobBuffer;
    }
}
