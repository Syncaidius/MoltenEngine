using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>
    /// TTF/OTF class definition table. See: https://www.microsoft.com/typography/otspec/chapter2.htm#classDefTbl
    /// </summary>
    /// <typeparam name="T">The type that class definition values should represent.</typeparam>
    public class ClassDefinitionTable<T> where T : struct
    {
        public ushort Format { get; internal set; }

        /// <summary>Gets the starting ID within <see cref="Values"/>.</summary>
        public ushort StartGlyphID { get; internal set; } = ushort.MaxValue;

        /// <summary>Gets a array containing the class ID's of each glyph in the array. The ID of a glyph should be used as an index for the array.</summary>
        public T[] Values => _glyphClassIDs;

        T[] _glyphClassIDs;

        internal ClassDefinitionTable(BinaryEndianAgnosticReader reader, Logger log, TableHeader header, T[] classTranslationTable)
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
                log.WriteWarning($"Unsupported Class-Definition sub-table in font '{header.Tag}' table");
            }
        }
    }
}
