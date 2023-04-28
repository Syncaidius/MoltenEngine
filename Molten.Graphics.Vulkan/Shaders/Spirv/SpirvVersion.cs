using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Vulkan
{
    public struct SpirvVersion
    {
        uint _version;

        public SpirvVersion(uint word)
        {
            _version = word;
        }

        public uint Variant => (_version >> 24) & 0xFF;

        public uint Major => (_version >> 16) & 0xFF;

        public uint Minor => (_version >> 8) & 0xFF;

        public uint Revision => _version & 0xFF;

        public override string ToString()
        {
            return $"{Variant}.{Major}.{Minor}.{Revision}";
        }

        public static explicit operator uint(SpirvVersion version)
        {
            return version._version;
        }

        public static explicit operator SpirvVersion(uint version)
        {
            return new SpirvVersion(version);
        }
    }
}
