namespace Molten.Graphics
{
    /// <summary>
    /// Matches the same memory layout as VK_MAKE_API_VERSION.
    /// </summary>
    public struct VersionVK
    {
        uint _value;

        public VersionVK(uint value)
        {
            _value = value;
        }

        public VersionVK(uint major, uint minor, uint revision) : 
            this(0, major, minor, revision) { }

        public VersionVK(uint variant, uint major, uint minor, uint revision)
        {
            if (variant > 7) // 3-bit
                throw new Exception("Variant must be between 0 and 7");

            if (major > 127) // 7-bit
                throw new Exception("Major must be between 0 and 127");

            if(minor > 1023) // 10-bit
                throw new Exception("Major must be between 0 and 1023");

            if (minor > 4096) // 12-bit
                throw new Exception("Major must be between 0 and 1023");

            _value = (variant << 29) | (major << 22) | (minor << 12) | revision;
        }

        public static implicit operator uint(VersionVK v)
        {
            return v._value;
        }

        public static implicit operator VersionVK(uint value)
        {
            return new VersionVK(value);
        }

        public override string ToString()
        {
            if (Variant > 0)
                return $"{Variant}.{Major}.{Minor}.{Revision}";
            else
                return $"{Major}.{Minor}.{Revision}";
        }

        public uint Variant => (_value >> 29);
        public uint Major => (_value >> 22) & 0x7FU;
        public uint Minor => (_value >> 12) & 0x3FFU;
        public uint Revision => _value & 0xFFFU;
    }
}
