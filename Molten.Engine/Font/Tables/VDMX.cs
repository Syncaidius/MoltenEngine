using Molten.IO;

namespace Molten.Font;

/// <summary>VDMX - Vertical Device Metrics table; Relates to OpenType™ fonts with TrueType outlines. <para/>
/// Under Windows, the usWinAscent and usWinDescent values from the 'OS/2' table will be used to determine the maximum black height for a font at any given size. <para/>
/// Windows calls this distance the Font Height. Because TrueType instructions can lead to Font Heights that differ from the actual scaled and rounded values, basing the Font Height strictly on the yMax and yMin can result in “lost pixels.” 
/// Windows will clip any pixels that extend above the yMax or below the yMin. In order to avoid grid fitting the entire font to determine the correct height, the VDMX table has been defined.<para/>
/// See: https://docs.microsoft.com/en-us/typography/opentype/spec/vdmx </summary>
[FontTableTag("VDMX")]
public class VDMX : FontTable
{
    public ushort Version { get; private set; }

    public RatioRangeRecord[] RatioRanges { get; private set; }

    public VDMXGroup[] RatioGroups { get; private set; }

    internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
    {
        Version = reader.ReadUInt16();
        ushort numRecs = reader.ReadUInt16();
        ushort numRatios = reader.ReadUInt16();

        // Populate ratios
        RatioRanges = new RatioRangeRecord[numRatios];
        for (int i = 0; i < numRatios; i++)
        {
            RatioRanges[i] = new RatioRangeRecord()
            {
                BCharSet = reader.ReadByte(),
                XRatio = reader.ReadByte(),
                YStartRatio = reader.ReadByte(),
                YEndRatio = reader.ReadByte(),
            };
        }

        // Populate groups
        ushort[] offsets = reader.ReadArray<ushort>(numRatios);
        RatioGroups = new VDMXGroup[numRatios];
        for (int i = 0; i < numRatios; i++)
            RatioGroups[i] = new VDMXGroup(reader, log, this, offsets[i]);
    }
}

/// <summary>
/// Represents an aspect ratio range.<para/>
/// All values set to zero signal the default grouping to use; if present, this must be the last Ratio group in the table. Ratios of 2:2 are the same as 1:1.
/// <para>
/// Ratios are set up as follows: <para/>
/// For a 1:1 aspect ratio  Ratios.xRatio = 1; Ratios.yStartRatio = 1; Ratios.yEndRatio = 1; <para/>
/// For 1:1 through 2:1 ratio Ratios.xRatio = 2; Ratios.yStartRatio = 1; Ratios.yEndRatio = 2; <para/>
/// For 1.33:1 ratio Ratios.xRatio = 4; Ratios.yStartRatio = 3; Ratios.yEndRatio = 3; <para/>
/// For _all_ aspect ratios Ratio.xRatio = 0; Ratio.yStartRatio = 0; Ratio.yEndRatio = 0; <para/>
/// </para>
/// </summary>
public class RatioRangeRecord
{
    /// <summary>
    /// Gets the character set value.
    /// </summary>
    public byte BCharSet { get; internal set; }

    /// <summary>
    /// Gets the value to use for x-Ratio.
    /// </summary>
    public byte XRatio { get; internal set; }

    /// <summary>
    /// Gets the starting y-Ratio value.
    /// </summary>
    public byte YStartRatio { get; internal set; }

    /// <summary>
    /// Gets the ending y-Ratio value
    /// </summary>
    public byte YEndRatio { get; internal set; }
}

/// <summary>
/// VDMX group. This table must appear in sorted order (sorted by yPelHeight), but need not be continous. It should have an entry for every pel height where the yMax and yMin do not scale linearly, where linearly scaled heights are defined as: <para/>
/// Hinted yMax and yMin are identical to scaled/rounded yMax and yMin
/// </summary>
public class VDMXGroup : FontSubTable
{
    public byte StartZ { get; private set; }

    public byte EndZ { get; private set; }

    public VDMXRecord[] Records { get; private set; }

    internal VDMXGroup(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset) :
        base(reader, log, parent, offset)
    {
        // MS Docs: It is assumed that once yPelHeight reaches 255, all heights will be linear, or at least close enough to linear that it no longer matters. 
        // Please note that while the Ratios structure can only support ppem sizes up to 255, the vTable structure can support much larger pel heights (up to 65535). 
        // The choice of int16 and uint16 for the vTable record is dictated by the requirement that yMax and yMin be signed values (and 127 to -128 is too small a range) and the desire to word-align the vTable elements.

        ushort recs = reader.ReadUInt16();
        StartZ = reader.ReadByte();
        EndZ = reader.ReadByte();
        Records = new VDMXRecord[recs];
        for (int i = 0; i < recs; i++)
        {
            Records[i] = new VDMXRecord()
            {
                YPerHeight = reader.ReadUInt16(),
                YMax = reader.ReadInt16(),
                YMin = reader.ReadInt16(),
            };
        }
    }
}

/// <summary>
/// VDMX group record.<para/>
/// </summary>
public class VDMXRecord
{
    public ushort YPerHeight { get; internal set; }

    public short YMax { get; internal set; }

    public short YMin { get; internal set; }
}
