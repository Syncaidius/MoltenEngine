using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class ItemVariationStore
    {
        public ushort Format { get; private set; }

        public VariationRegionList RegionList { get; private set; }

        /// <summary>Gets an array of item variation data subtable. Each includes deltas for some number of items, and some subset of regions. <para/>
        /// The regions are indicated by an array of indices into the variation region list.</summary>
        public ItemVariationData[] DeltaSets { get; private set; }

        internal ItemVariationStore(BinaryEndianAgnosticReader reader, Logger log, TableHeader parentHeader)
        {
            long startOffset = reader.Position;

            Format = reader.ReadUInt16();
            uint variationRegionListOffset = reader.ReadUInt32();
            ushort itemVariationDataCount = reader.ReadUInt16();

            uint[] ivdOffsets = reader.ReadArrayUInt32(itemVariationDataCount);
            DeltaSets = new ItemVariationData[itemVariationDataCount];

            // Read IVD sub-tables
            for(int i = 0; i < itemVariationDataCount; i++)
            {
                reader.Position = startOffset + ivdOffsets[i];
                DeltaSets[i] = new ItemVariationData(reader, log);
            }

            // Read region list
            reader.Position = startOffset + variationRegionListOffset;
            RegionList = new VariationRegionList(reader, log);
        }
    }

    public class ItemVariationData
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

        internal ItemVariationData(BinaryEndianAgnosticReader reader, Logger log)
        {
            ItemCount = reader.ReadUInt16();
            ShortDeltaCount = reader.ReadUInt16();
            RegionIndexCount = reader.ReadUInt16();

            TotalDeltaSetColumns = ShortDeltaCount + RegionIndexCount;
            DeltaSets = new short[ItemCount, TotalDeltaSetColumns];

            // Read region indices
            RegionIndices = reader.ReadArrayUInt16(RegionIndexCount);

            // Read delta sets
            for(int i = 0; i < ItemCount; i++)
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

    public class VariationRegionList
    {
        /// <summary>Gets a multi-dimensional array containing region axis data. <para />
        /// The first dimension is the region ID. The second dimension is the axis ID.</summary>
        public Axis[,] RegionAxes { get; private set; }

        public class Axis
        {
            public float StartCoord { get; internal set; }

            public float PeakCoord { get; internal set; }

            public float EndCoord { get; internal set; }
        }

        internal VariationRegionList(BinaryEndianAgnosticReader reader, Logger log)
        {
            ushort axisCount = reader.ReadUInt16();
            ushort regionCount = reader.ReadUInt16();

            RegionAxes = new Axis[regionCount, axisCount];

            for (int r = 0; r < regionCount; r++)
            {
                for (int a = 0; a < axisCount; a++)
                {
                    RegionAxes[r, a] = new Axis()
                    {
                        StartCoord = FontMath.FromF2DOT14(reader.ReadInt16()),
                        PeakCoord = FontMath.FromF2DOT14(reader.ReadInt16()),
                        EndCoord = FontMath.FromF2DOT14(reader.ReadInt16()),
                    };
                }
            }
        }
    }
}
