namespace Molten.Font
{
    [Flags]
    /// <summary>A font's mac-style.</summary>
    public enum MacStyleFlags : ushort
    {
        None = 0,

        Bold = 1,

        Italic = 2,

        Underline = 4,

        Outline = 8,

        Shadow = 16,

        Condensed = 32,

        Extended = 64,

        Reserved7 = 128,

        Reserved8 = 256,

        Reserved9 = 512,

        Reserved10 = 1024,

        Reserved11 = 2048,

        Reserved12 = 4096,

        Reserved13 = 8192,

        Reserved14 = 16384,

        Reserved15 = 32768,
    }
}
