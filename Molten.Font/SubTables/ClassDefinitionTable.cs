using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>
    /// TTF/OTF class definition table which stores class values as a <see cref="ushort"/>. See: https://www.microsoft.com/typography/otspec/chapter2.htm#classDefTbl
    /// </summary>
    public class ClassDefinitionTable : FontSubTable
    {
        public ushort Format { get; internal set; }

        /// <summary>Gets the starting ID within <see cref="Values"/>.</summary>
        public ushort StartGlyphID { get; internal set; } = ushort.MaxValue;

        /// <summary>Gets a array containing the class ID's of each glyph in the array. The ID of a glyph should be used as an index for the array.</summary>
        public ushort[] Values => _glyphClassIDs;

        ushort[] _glyphClassIDs;

        internal ClassDefinitionTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            Format = reader.ReadUInt16();

            if (Format == 1) // ClassDefFormat1
            {
                StartGlyphID = reader.ReadUInt16();
                ushort glyphCount = reader.ReadUInt16();
                _glyphClassIDs = reader.ReadArrayUInt16(glyphCount);
            }
            else if (Format == 2) // ClassDefFormat2
            {
                ushort classRangeCount = reader.ReadUInt16();
                for (ushort i = 0; i < classRangeCount; i++)
                {
                    ushort glyphStartID = reader.ReadUInt16();
                    ushort glyphEndID = reader.ReadUInt16();
                    ushort glyphClass = reader.ReadUInt16();

                    StartGlyphID = Math.Min(glyphStartID, StartGlyphID);
                    if (Values == null || glyphEndID >= Values.Length)
                        Array.Resize(ref _glyphClassIDs, glyphEndID + 1);

                    for (int g = glyphStartID; g <= glyphEndID; g++)
                        _glyphClassIDs[g] = glyphClass;
                }
            }
            else
            {
                log.WriteWarning($"Unsupported Class-Definition sub-table format: {Format}");
            }
        }
    }

    /// <summary>
    /// TTF/OTF class definition table which stores class values as type T. See: https://www.microsoft.com/typography/otspec/chapter2.htm#classDefTbl
    /// </summary>
    /// <typeparam name="T">The type that class definition values should represent.</typeparam>
    public class ClassDefinitionTable<T> : FontSubTable where T : struct
    {
        public ushort Format { get; internal set; }

        /// <summary>Gets the starting ID within <see cref="Values"/>.</summary>
        public ushort StartGlyphID { get; internal set; } = ushort.MaxValue;

        /// <summary>Gets a array containing the class ID's of each glyph in the array. The ID of a glyph should be used as an index for the array.</summary>
        public T[] Values => _glyphClassIDs;

        T[] _glyphClassIDs;

        internal ClassDefinitionTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset, T[] classTranslationTable) :
            base(reader, log, parent, offset)
        {
            Format = reader.ReadUInt16();

            if (Format == 1) // ClassDefFormat1
            {
                StartGlyphID = reader.ReadUInt16();
                ushort glyphCount = reader.ReadUInt16();
                _glyphClassIDs = new T[glyphCount];
                for (ushort i = 0; i < glyphCount; i++)
                    Values[i] = classTranslationTable[reader.ReadUInt16()];
            }
            else if (Format == 2) // ClassDefFormat2
            {
                ushort classRangeCount = reader.ReadUInt16();
                for (ushort i = 0; i < classRangeCount; i++)
                {
                    ushort glyphStartID = reader.ReadUInt16();
                    ushort glyphEndID = reader.ReadUInt16();
                    T glyphClass = classTranslationTable[reader.ReadUInt16()];

                    StartGlyphID = Math.Min(glyphStartID, StartGlyphID);
                    if (Values == null || glyphEndID >= Values.Length)
                        Array.Resize(ref _glyphClassIDs, glyphEndID + 1);

                    for(int g = glyphStartID; g <= glyphEndID; g++)
                        _glyphClassIDs[g] = glyphClass;
                }
            }
            else
            {
                log.WriteWarning($"Unsupported Class-Definition sub-table format: {Format}");
            }
        }
    }
}
