using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class FeatureTableSubstitutionTable : FontSubTable
    {
        public ushort MajorVersion { get; internal set; }

        public ushort MinorVersion { get; internal set; }

        public FeatureTableSubstitutionRecord[] Records { get; internal set; }

        internal FeatureTableSubstitutionTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            ushort substitutionCount = reader.ReadUInt16();
            uint[] altFeatureOffsets = new uint[substitutionCount];
            Records = new FeatureTableSubstitutionRecord[substitutionCount];
            for (int i = 0; i < substitutionCount; i++)
            {
                Records[i] = new FeatureTableSubstitutionRecord()
                {
                    FeatureIndex = reader.ReadUInt16(),
                };

                altFeatureOffsets[i] = reader.ReadUInt32();
            }

            // Populate record tables
            // TODO track loaded feature lists in font and use an existing instance here if possible.
            //      Might be loading the same feature table data into multiple FeatureTable instances.
            for (int i = 0; i < substitutionCount; i++)
                Records[i].AlternateFeatureTable = new FeatureTable(reader, log, this, altFeatureOffsets[i]);
        }
    }

    public class FeatureTableSubstitutionRecord
    {
        public ushort FeatureIndex { get; internal set; }

        /// <summary>
        /// Gets an alternate feature table.
        /// </summary>
        public FeatureTable AlternateFeatureTable { get; internal set; }
    }
}
