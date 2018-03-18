using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>A sub-table for the <see cref="Kern"/> font table.</summary>
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
        /// Only available when <see cref="KerningTableFlags.Format0"/> is present.
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
                case 0:
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

                case 2:
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

    public class KerningPair
    {
        /// <summary>
        /// Gets the glyph index (ID) for the left-hand glyph in the kerning pair.
        /// </summary>
        public ushort Left { get; internal set; }

        /// <summary>
        /// Gets the glyph index (ID) for the right-hand glyph in the kerning pair.
        /// </summary>
        public ushort Right { get; internal set; }

        /// <summary>
        /// Gets the kerning value for the above pair, in FUnits. <para/>
        /// If this value is greater than zero, the characters will be moved apart. If this value is less than zero, the character will be moved closer together.
        /// </summary>
        public short Value { get; internal set; }
    }

    public enum KerningTableFlags : byte
    {
        None = 0,

        Horizonal = 1,

        Minimum = 2,

        /// <summary>
        /// If set to 1, kerning is perpendicular to the flow of the text. <para/>
        /// If the text is normally written horizontally, kerning will be done in the up and down directions. <para/>
        /// If kerning values are positive, the text will be kerned upwards; if they are negative, the text will be kerned downwards. If the text is normally written vertically, kerning will be done in the left and right directions. If kerning values are positive, the text will be kerned to the right; if they are negative, the text will be kerned to the left. <para/>
        /// The value 0x8000 in the kerning data resets the cross-stream kerning back to 0.
        /// </summary>
        CrossStream = 4,

        Override = 8,

        Reserved4 = 16,

        Reserved5 = 32,

        Reserved6 = 64,

        Reserved7 = 128,
    }
}
