using Molten.IO;

namespace Molten.Font
{
    public class ItemVariationStore : FontSubTable
    {
        public ushort Format { get; private set; }

        public VariationRegionListTable RegionList { get; private set; }

        /// <summary>Gets an array of item variation data subtable. Each includes deltas for some number of items, and some subset of regions. <para/>
        /// The regions are indicated by an array of indices into the variation region list.</summary>
        public ItemVariationData[] DeltaSets { get; private set; }

        internal ItemVariationStore(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            Format = reader.ReadUInt16();
            uint variationRegionListOffset = reader.ReadUInt32();
            ushort itemVariationDataCount = reader.ReadUInt16();

            uint[] ivdOffsets = reader.ReadArray<uint>(itemVariationDataCount);
            DeltaSets = new ItemVariationData[itemVariationDataCount];

            // Read IVD sub-tables
            for (int i = 0; i < itemVariationDataCount; i++)
                DeltaSets[i] = new ItemVariationData(reader, log, this, ivdOffsets[i]);

            // Read region list
            RegionList = new VariationRegionListTable(reader, log, this, variationRegionListOffset);
        }
    }

    public class ItemVariationData : FontSubTable
    {
        /// <summary>Gets the delta set table. The number of rows is equal to <see cref="ItemCount"/>, while the number of columns is equal to <see cref="TotalDeltaSetColumns"/>.</summary>
        public short[,] DeltaSets { get; private set; }

        /// <summary>Gets an array of indices into the variation region list for the regions referenced by the current <see cref="ItemVariationData"/> table.</summary>
        public ushort[] RegionIndices { get; private set; }

        /// <summary>Gets the number of items/rows in <see cref="DeltaSets"/>.</summary>
        public ushort ItemCount { get; private set; }

        /// <summary>Gets the number of signed 16-bit deltas within <see cref="DeltaSets"/>.</summary>
        public ushort ShortDeltaCount { get; private set; }

        /// <summary>Gets the number of signed 8-bit variation region indices in <see cref="DeltaSets"/>, which are placed after <see cref="ShortDeltaCount"/> elements in <see cref="DeltaSets"/>.</summary>
        public ushort RegionIndexCount { get; private set; }

        /// <summary>Gets the sum of <see cref="ShortDeltaCount"/> and <see cref="RegionIndexCount"/>.</summary>
        public int TotalDeltaSetColumns { get; private set; }

        internal ItemVariationData(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
            base(reader, log, parent, offset)
        {
            ItemCount = reader.ReadUInt16();
            ShortDeltaCount = reader.ReadUInt16();
            RegionIndexCount = reader.ReadUInt16();

            TotalDeltaSetColumns = ShortDeltaCount + RegionIndexCount;
            DeltaSets = new short[ItemCount, TotalDeltaSetColumns];

            // Read region indices
            RegionIndices = reader.ReadArray<ushort>(RegionIndexCount);

            // Read delta sets
            for (int i = 0; i < ItemCount; i++)
            {
                // Read 16-bit values first (short)
                for (int d = 0; d < ShortDeltaCount; d++)
                    DeltaSets[i, d] = reader.ReadInt16();

                // Now read 8-bit signed values (sbyte)
                for (int d = ShortDeltaCount; d < TotalDeltaSetColumns; d++)
                    DeltaSets[i, d] = reader.ReadSByte();
            }
        }
    }
}
