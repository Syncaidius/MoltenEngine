﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public partial class GPOS
    {
        public class PairSet
        {
            public PairValueRecord[] PairRecords { get; private set; }

            internal PairSet(BinaryEndianAgnosticReader reader, long startPos, ValueFormat format1, ValueFormat format2)
            {
                reader.Position = startPos;
                ushort pairvalueCount = reader.ReadUInt16();
                PairRecords = new PairValueRecord[pairvalueCount];
                for (int i = 0; i < pairvalueCount; i++)
                {
                    PairRecords[i] = new PairValueRecord()
                    {
                        SecondGlyphID = reader.ReadUInt16(),
                        Record1 = new ValueRecord(reader, format1),
                        Record2 = new ValueRecord(reader, format2),
                    };
                }
            }
        }
        public class PairValueRecord
        {
            /// <summary>
            /// Gets the glyph ID of second glyph in the pair. 
            /// The ID of the first glyph is listed is listed in the Coverage table of the parent <see cref="PairAdjustmentPositioningSubTable"/>.
            /// </summary>
            public ushort SecondGlyphID { get; internal set; }

            /// <summary>
            /// Gets the positioning data for the first glyph in the pair.
            /// </summary>
            public ValueRecord Record1 { get; internal set; }

            /// <summary>
            /// Gets the positioning data for the second glyph in the pair.
            /// </summary>
            public ValueRecord Record2 { get; internal set; }
        }
    }
}
