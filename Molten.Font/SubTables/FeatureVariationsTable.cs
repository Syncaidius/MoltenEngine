using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class FeatureVariationsTable
    {
        struct RecordOffsets
        {
            public uint ConditionSetOffset;

            public uint FeatureTableSubstitutionOffset;
        }

        public ushort MajorVersion { get; internal set; }

        public ushort MinorVersion { get; internal set; }

        public FeatureVariationRecord[] Records { get; internal set; }

        internal FeatureVariationsTable(BinaryEndianAgnosticReader reader, Logger log, long startPos)
        {
            reader.Position = startPos;
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
                    ConditionSet = new ConditionSetTable(reader, log, startPos + offsets[i].ConditionSetOffset),
                    FeatureVarSubsitutionTable = new FeatureTableSubstitutionTable(reader, log, startPos + offsets[i].FeatureTableSubstitutionOffset),
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

    public class ConditionSetTable
    {
        public ConditionTable[] Tables { get; internal set; }

        internal ConditionSetTable(BinaryEndianAgnosticReader reader, Logger log, long startPos)
        {
            reader.Position = startPos;
            ushort conditionCount = reader.ReadUInt16();
            uint[] conditionTableOffsets = reader.ReadArrayUInt32(conditionCount);
            Tables = new ConditionTable[conditionCount];
            for (int i = 0; i < conditionCount; i++)
                Tables[i] = new ConditionTable(reader, log, startPos + conditionTableOffsets[i]);
        }
    }

    public class ConditionTable
    {
        /// <summary>
        /// Gets the format of the current <see cref="ConditionTable"/>.
        /// </summary>
        public ushort Format { get; internal set; }

        /// <summary>
        /// Gets the index (zero-based) for the variation axis within the 'fvar' table.
        /// </summary>
        public ushort AxisIndex { get; internal set; }

        /// <summary>
        /// Gets the minimum value of the font variation instances that satisfy this condition.
        /// </summary>
        public float FilterRangeMinValue { get; internal set; }

        /// <summary>
        /// Gets the maximum value of the font variation instances that satisfy this condition.
        /// </summary>
        public float FilterRangeMaxValue { get; internal set; }

        internal ConditionTable(BinaryEndianAgnosticReader reader, Logger log, long startPos)
        {
            reader.Position = startPos;
            Format = reader.ReadUInt16();

            switch (Format)
            {
                case 1:
                    AxisIndex = reader.ReadUInt16();
                    FilterRangeMinValue = FontUtil.FromF2DOT14(reader.ReadUInt16());
                    FilterRangeMaxValue = FontUtil.FromF2DOT14(reader.ReadUInt16());
                    break;
            }
        }
    }

    public class FeatureTableSubstitutionTable
    {
        public ushort MajorVersion { get; internal set; }

        public ushort MinorVersion { get; internal set; }

        public FeatureTableSubstitutionRecord[] Records { get; internal set; }

        internal FeatureTableSubstitutionTable(BinaryEndianAgnosticReader reader, Logger log, long startPos)
        {
            reader.Position = startPos;
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            ushort substitutionCount = reader.ReadUInt16();
            uint[] altFeatureOffsets = new uint[substitutionCount];
            Records = new FeatureTableSubstitutionRecord[substitutionCount];
            for(int i = 0; i < substitutionCount; i++)
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
                Records[i].AlternateFeatureTable = new FeatureTable(reader, log, startPos + altFeatureOffsets[i]);
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
