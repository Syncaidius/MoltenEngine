using Molten.IO;

namespace Molten.Font;

public class VariationRegionListTable : FontSubTable
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

    internal VariationRegionListTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
        base(reader, log, parent, offset)
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
                    StartCoord = FontUtil.FromF2DOT14(reader.ReadInt16()),
                    PeakCoord = FontUtil.FromF2DOT14(reader.ReadInt16()),
                    EndCoord = FontUtil.FromF2DOT14(reader.ReadInt16()),
                };
            }
        }
    }
}
