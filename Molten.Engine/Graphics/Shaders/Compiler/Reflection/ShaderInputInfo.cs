using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class ShaderInputInfo
    {
        public string Name;

        public uint BindPoint;

        public uint BindCount;

        public ShaderInputType InputType;

        public ShaderReturnType ResourceReturnType;

        public ShaderResourceDimension Dimension;

        public uint NumSamples;

        public ShaderInputFlags Flags;

        public bool HasInputFlags(ShaderInputFlags flags)
        {
            return (Flags & flags) == flags;
        }
    }
}
