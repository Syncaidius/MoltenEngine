using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>A base implementation for GPOS and GSUB tables.</summary>
    public abstract class FontGTable : FontTable
    {
        public ushort MajorVersion { get; internal set; }

        public ushort MinorVersion { get; internal set; }

        /// <summary>Gets the <see cref="ScriptList"/> associated with the current table.</summary>
        public ScriptListTable ScriptList { get; internal set; }

        /// <summary>
        /// Gets the <see cref="FeatureListTable"/> associated with the current table.
        /// </summary>
        public FeatureListTable FeatureList { get; internal set; }

        /// <summary>
        /// Gets the <see cref="LookupListTable"/> associated with the current table.
        /// </summary>
        public LookupListTable LookupTable { get; internal set; }

        /// <summary>
        /// Gets the feature variations table associated with the current table. Optional (may be null).
        /// </summary>
        public FeatureVariationsTable FeatureVarTable { get; internal set; }
        
        protected abstract Type[] GetLookupTypeIndex();

        protected abstract ushort GetExtensionIndex();

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            Type[] lookupIndex = GetLookupTypeIndex();

            /* Certain structures are used across multiple GPOS Lookup subtable types and formats. All Lookup subtables use the Coverage table, 
             * which is defined in the OpenType Layout Common Table Formats chapter. 
             * Single and pair adjustments (LookupTypes 1 and 2) use a ValueRecord structure and associated ValueFormat enumeration, which are defined later in this chapter. 
             * Attachment subtables (LookupTypes 3, 4, 5 and 6) use Anchor and MarkArray tables, also defined later in this chapter.*/

            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            ushort scriptListOffset = reader.ReadUInt16();
            ushort featureListOffset = reader.ReadUInt16();
            ushort lookupListOffset = reader.ReadUInt16();
            uint featureVariationsOffset = 0;

            // Version 1.1 - Optional eature variation table.
            if (MajorVersion == 1 && MinorVersion == 1)
            {
                featureVariationsOffset = reader.ReadUInt32();
                if (featureVariationsOffset > FontUtil.NULL)
                    FeatureVarTable = new FeatureVariationsTable(reader, log, this, featureVariationsOffset);
            }

            ushort extensionIndex = GetExtensionIndex();
            ScriptList = new ScriptListTable(reader, log, this, scriptListOffset);
            FeatureList = new FeatureListTable(reader, log, this, featureListOffset);
            LookupTable = new LookupListTable(reader, log, this, lookupListOffset, lookupIndex, extensionIndex);
            reader.Position = header.ReadOffset + header.Length;
        }
    }
}
