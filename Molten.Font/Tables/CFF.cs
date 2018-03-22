using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Index-to-location table.<para/>
    /// <para>The indexToLoc table stores the offsets to the locations of the glyphs in the font, relative to the beginning of the glyphData table. In order to compute the length of the last glyph element, there is an extra entry after the last valid index.</para>
    /// <para>By definition, index zero points to the "missing character," which is the character that appears if a character is not found in the font. The missing character is commonly represented by a blank box or a space. If the font does not contain an outline for the missing character, then the first and second offsets should have the same value. This also applies to any other characters without an outline, such as the space character. If a glyph has no outline, then loca[n] = loca [n+1]. In the particular case of the last glyph(s), loca[n] will be equal the length of the glyph data ('glyf') table. The offsets must be in ascending order with loca[n] less-or-equal-to loca[n+1].</para>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/loca </summary>
    [FontTableTag("CFF")]
    public class CFF : FontTable
    {
        public byte MajorVersion { get; private set; }

        public byte MinorVersion { get; private set; }

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            // Read header
            MajorVersion = reader.ReadByte();
            MinorVersion = reader.ReadByte();

            byte headerSize = reader.ReadByte();
            byte offSize = reader.ReadByte();
            if(MajorVersion == 1 && MinorVersion == 0)
            {
                IndexTable nameIndex = new IndexTable(reader, log, this, headerSize);
                IndexTable topDictIndex = new IndexTable(reader, log, this, nameIndex.OffsetToNextBlock);
                IndexTable stringIndex = new IndexTable(reader, log, this, topDictIndex.OffsetToNextBlock);
            }
            else
            {
                log.WriteDebugLine($"[CFF] Unsupported CFF version {MajorVersion}.{MinorVersion}");
            }
        }

        /// <summary>
        /// CFF index table. Contains an array of offsets and the offset to the next block of CFF data.
        /// </summary>
        private class IndexTable : FontSubTable
        {
            public ObjectDataOffset[] Objects;

            public long OffsetToNextBlock;

            internal IndexTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) : 
                base(reader, log, parent, offset)
            {
                ushort count = reader.ReadUInt16();
                Objects = new ObjectDataOffset[count];

                // An empty INDEX is represented by a count field with a 0 value and no additional fields. Thus, the total size of an empty INDEX is 2 bytes.
                if (count == 0)
                    return;

                byte offsetSize = reader.ReadByte();
                uint nextOffset = 0;
                uint curOffset = 0;
                uint offsetArraySize = offsetSize * (count + 1U);
                uint headerSize = GetLocalOffset(reader);
                uint objectDataOffset = headerSize + offsetArraySize;
                uint objectDataStreamOffset = objectDataOffset + (uint)Header.StreamOffset;

                switch (offsetSize)
                {
                    case 1:
                        curOffset = objectDataStreamOffset + (reader.ReadByte() - 1U);
                        for (int i = 0; i < count; i++)
                        {
                            nextOffset = objectDataStreamOffset + (reader.ReadByte() - 1U);
                            Objects[i] = new ObjectDataOffset()
                            {
                                Offset = curOffset,
                                DataSize = nextOffset - curOffset,
                            };

                            curOffset = nextOffset;
                        }
                        break;

                    case 2:
                        curOffset = objectDataStreamOffset + (reader.ReadUInt16() - 1U);
                        for (int i = 0; i < count; i++)
                        {
                            nextOffset = objectDataStreamOffset + (reader.ReadUInt16() - 1U);
                            Objects[i] = new ObjectDataOffset()
                            {
                                Offset = curOffset,
                                DataSize = nextOffset - curOffset,
                            };

                            curOffset = nextOffset;
                        }
                        break;

                    case 4:
                        curOffset = objectDataStreamOffset + (reader.ReadUInt32() - 1U);
                        for (int i = 0; i < count; i++)
                        {
                            nextOffset = objectDataStreamOffset + (reader.ReadUInt32() - 1U);
                            Objects[i] = new ObjectDataOffset()
                            {
                                Offset = curOffset,
                                DataSize = nextOffset - curOffset,
                            };

                            curOffset = nextOffset;
                        }
                        break;
                }

                OffsetToNextBlock = nextOffset;
            }
        }

        internal class ObjectDataOffset
        {
            public uint Offset;

            public uint DataSize;
        }
    }

}
