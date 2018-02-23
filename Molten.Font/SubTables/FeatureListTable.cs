using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>
    /// A feature list table.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/chapter2#feature-table
    /// </summary>
    public class FeatureListTable
    {
        /// <summary>
        /// Gets an array of <see cref="FeatureRecord"/> — zero-based (first feature has FeatureIndex = 0), listed alphabetically by <see cref="FeatureRecord.Tag"/>.
        /// </summary>
        public FeatureRecord[] Records { get; internal set; }

        internal FeatureListTable(BinaryEndianAgnosticReader reader, Logger log, long startPos)
        {
            reader.Position = startPos;
            ushort featureCount = reader.ReadUInt16();
            Records = new FeatureRecord[featureCount];
            ushort[] featureOffsets = new ushort[featureCount];

            // Prepare records and collect their feature table offsets.
            for(int i = 0; i < featureCount; i++)
            {
                Records[i] = new FeatureRecord()
                {
                    Tag = FontUtil.ReadTag(reader),
                };
                featureOffsets[i] = reader.ReadUInt16();
            }

            // Now populate the record tables using offsets collected above.
            for (int i = 0; i < featureCount; i++)
                Records[i].Table = new FeatureTable(reader, log, startPos + featureOffsets[i]);
        }
    }

    public class FeatureRecord
    {
        public string Tag { get; internal set; }

        /// <summary>
        /// Gets the <see cref="FeatureTable"/> associated with the current record.
        /// </summary>
        public FeatureTable Table { get; internal set; }
    }

    /// <summary>
    /// A feature table.<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/chapter2#feature-table
    /// </summary>
    public class FeatureTable
    {
        /// <summary>
        /// Gets the feature params value. Reserved for FeatureParams, so usually equal to <see cref="FontUtil.NULL"/>.
        /// </summary>
        public ushort FeatureParams { get; internal set; }

        /// <summary>
        /// Gets an array of indices into the LookupList — zero-based (first lookup is LookupListIndex = 0)
        /// </summary>
        public ushort[] LookupListIndices { get; internal set; }

        internal FeatureTable(BinaryEndianAgnosticReader reader, Logger log, long startPos)
        {
            reader.Position = startPos;
            FeatureParams = reader.ReadUInt16();
            ushort lookupIndexCount = reader.ReadUInt16();
            LookupListIndices = reader.ReadArrayUInt16(lookupIndexCount);
        }
    }
}
