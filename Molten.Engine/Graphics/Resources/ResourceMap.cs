using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Core.Attributes;

namespace Molten.Graphics
{
    public struct ResourceMap
    {
        public unsafe void* Ptr;

        public uint RowPitch;

        public uint DepthPitch;

        public unsafe ResourceMap(void* ptr, uint rowPitch, uint depthPitch)
        {
            Ptr = ptr;
            RowPitch = rowPitch;
            DepthPitch = depthPitch;
        }
    }
}
