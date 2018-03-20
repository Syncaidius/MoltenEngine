using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class FeatureVariationsTable : FontSubTable
    {
        struct RecordOffsets
        {
            public uint ConditionSetOffset;

            public uint FeatureTableSubstitutionOffset;
        }

        public ushort MajorVersion { get; internal set; }

        public ushort MinorVersion { get; internal set; }

        public FeatureVariationRecord[] Records { get; internal set; }

        internal override void Read(EnhancedBinaryReader reader, FontReaderContext context, FontTable parent)
        {
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();

            uint recordCount = reader.ReadUInt32();
            Records = new FeatureVariationRecord[recordCount];
            RecordOffsets[] offsets = new RecordOffsets[recordCount];
            for(int i = 0; i < recordCount; i++)
            {
                offsets[i] = new RecordOffsets()
                {
                    ConditionSetOffset = reader.ReadUInt32(),
                    FeatureTableSubstitutionOffset = reader.ReadUInt32(),
                };
            }

            // Populate records
            for(int i = 0; i < recordCount; i++)
            {
                Records[i] = new FeatureVariationRecord()
                {
                    ConditionSet = context.ReadSubTable<ConditionSetTable>(offsets[i].ConditionSetOffset),
                    FeatureVarSubsitutionTable = context.ReadSubTable<FeatureTableSubstitutionTable>(offsets[i].FeatureTableSubstitutionOffset),
                };
            }
        }
    }

    public class FeatureVariationRecord
    {
        /// <summary>
        /// Gets the <see cref="ConditionSetTable"/> associated with the current record.
        /// </summary>
        public ConditionSetTable ConditionSet { get; internal set; }

        /// <summary>
        /// Gets the <see cref="FeatureTableSubstitutionTable"/> associated with the current record.
        /// </summary>
        public FeatureTableSubstitutionTable FeatureVarSubsitutionTable { get; internal set; }
    }
}
