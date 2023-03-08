namespace Molten.Graphics
{
    [Flags]
    public enum ColorWriteFlags : byte
    {
        None = 0,

        Red = 0x1,

        Green = 0x2,

        Blue = 0x4,

        Alpha = 0x8,

        All = 0xF
    }
}
