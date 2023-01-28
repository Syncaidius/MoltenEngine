namespace Molten.Graphics
{
    internal struct CharData
    {
        public ushort GlyphIndex;

        public bool Initialized;

        public CharData(ushort gIndex)
        {
            GlyphIndex = gIndex;
            Initialized = true;
        }
    }
}
