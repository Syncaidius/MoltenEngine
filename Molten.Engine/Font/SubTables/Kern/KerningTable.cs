using Molten.IO;

namespace Molten.Font;

/// <summary>A sub-table for the <see cref="Kern"/> font table.
/// <para>See: https://learn.microsoft.com/en-us/typography/opentype/spec/kern</para></summary>
public class KerningTable : FontSubTable
{
    public ushort Version { get; private set; }

    public KerningTableFlags Flags { get; private set; }

    public byte Format { get; private set; }

    public KerningPair[] Pairs { get; private set; }

    /// <summary>
    /// Gets the largest power of two less than or equal to the length of <see cref="Pairs"/>, multiplied by the size in bytes of an entry in the table.
    /// </summary>
    public ushort SearchRange { get; private set; }

    /// <summary>
    /// Gets the entry selector value. This is calculated as log2 of the largest power of two less than or equal to the value of nPairs. <para/>
    /// This value indicates how many iterations of the search loop will have to be made. (For example, in a list of eight items, there would have to be three iterations of the loop).
    /// </summary>
    public ushort EntrySelector { get; private set; }

    /// <summary>
    /// Gets the value of nPairs minus the largest power of two less than or equal to nPairs, and then multiplied by the size in bytes of an entry in the table.<para/>
    /// Only available when Kern is format 0.
    /// </summary>
    public ushort RangeShift { get; private set; }

    internal KerningTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
        base(reader, log, parent, offset)
    {
        Version = reader.ReadUInt16();
        ushort subHeaderLength = reader.ReadUInt16();

        // Read the uint16 coverage value as two separate bytes. Format first, then flags after.
        // This avoids bit-shifting it all around.
        Format = reader.ReadByte();
        Flags = (KerningTableFlags)reader.ReadByte();

        switch (Format)
        {
            case 0: // Format 0 - Ordered List of Kerning Pairs
                ushort numPairs = reader.ReadUInt16();
                SearchRange = reader.ReadUInt16();
                EntrySelector = reader.ReadUInt16();
                RangeShift = reader.ReadUInt16();

                Pairs = new KerningPair[numPairs];
                for (int i = 0; i < numPairs; i++)
                {
                    Pairs[i] = new KerningPair()
                    {
                        Left = reader.ReadUInt16(),
                        Right = reader.ReadUInt16(),
                        Value = reader.ReadInt16(),
                    };
                }
                break;

            case 1: // Format 1 - State Table for Contextual Kerning
                // TODO Add support for format 1 kerning tables. See: https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6kern.html
                break;

            case 2: // Format 2 - Simple n x m Array of Kerning Values
                ushort rowWidth = reader.ReadUInt16();
                ushort leftClassTableOffset = reader.ReadUInt16();
                ushort rightClasstableOffset = reader.ReadUInt16();
                ushort arrayOffset = reader.ReadUInt16();

                ushort[] leftClasses = ReadClassTable(reader, Header.StreamOffset, leftClassTableOffset);
                ushort[] rightClasses = ReadClassTable(reader, Header.StreamOffset, rightClasstableOffset);

                // "Un-multiply" the values in each class table to give us the original class values.
                // Left class table - The values in the left class table are stored pre-multiplied by the number of bytes in one row
                for (int i = 0; i < leftClasses.Length; i++)
                    leftClasses[i] /= rowWidth;

                // Right class table - The values in the right class table are stored pre-multiplied by the number of bytes in a single kerning value
                ushort int16Size = sizeof(short);
                for (int i = 0; i < rightClasses.Length; i++)
                    rightClasses[i] /= int16Size;

                short[,] kerningArray = new short[leftClasses.Length, rightClasses.Length];
                for (int r = 0; r < leftClasses.Length; r++)
                {
                    for (int c = 0; c < rightClasses.Length; c++)
                        kerningArray[r, c] = reader.ReadInt16();
                }

                // TODO translate into KerningPair instances -- Need a test font with format2 kerning tables.
                break;

            case 3: // Format 3 - Simple n x m Array of Kerning Indices
                // TODO Add support for format 3 kerning tables. See: https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6kern.html
                break;
        }
    }

    private ushort[] ReadClassTable(EnhancedBinaryReader reader, long tableStart, ushort offset)
    {
        reader.Position = tableStart + offset;
        ushort firstGlyph = reader.ReadUInt16(); // ID of the first glyph within the class range
        ushort numGlyphs = reader.ReadUInt16();
        ushort[] classTable = reader.ReadArray<ushort>(numGlyphs);

        return classTable;
    }
}
