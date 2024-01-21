using Molten.IO;

namespace Molten.Font;

public enum DeviceVariableTableFormat
{
    Invalid = 0,

    /// <summary>Signed 2-bit value, 8 values per uint16 </summary>
    Local2BitDeltas = 0x0001,

    /// <summary>Signed 4-bit value, 4 values per uint16 </summary>
    Local4BitDeltas = 0x0002,

    /// <summary>Signed 8-bit value, 2 values per uint16 </summary>
    Local8BitDeltas = 0x0003,

    /// <summary>VariationIndex table, contains a delta-set index pair. </summary>
    VariationIndex = 0x8000,

    /// <summary>For future use — set to 0</summary>
    Reserved = 0x7FFC,
}
public class DeviceVariationIndexTable : FontSubTable
{
    public DeviceVariableTableFormat Format { get; private set; }

    /// <summary>The smallest size to correct, in ppem (pixels per em). <para/>
    /// Only set when <see cref="Format"/> is not <see cref="DeviceVariableTableFormat.VariationIndex"/>.</summary>
    public ushort DeviceStartSize { get; private set; }

    /// <summary>The largest size to correct, in ppem (pixels per em). <para/>
    /// Only set when <see cref="Format"/> is not <see cref="DeviceVariableTableFormat.VariationIndex"/>.</summary>
    public ushort DeviceEndSize { get; private set; }

    /// <summary>A delta-set outer index — used to select an item variation data subtable within an item variation store. <para/>
    /// Only set when <see cref="Format"/> is set to <see cref="DeviceVariableTableFormat.VariationIndex"/>.</summary>
    public ushort DeltaSetOuterIndex { get; private set; }


    /// <summary>A delta-set inner index — used to select a delta-set row within an item variation data subtable. <para/>
    /// Only set when <see cref="Format"/> is set to <see cref="DeviceVariableTableFormat.VariationIndex"/>.</summary>
    public ushort DeltaSetInnerIndex { get; private set; }

    /// <summary>An array of de-compressed delta values. Each element corresponds to a ppem size between <see cref="DeviceStartSize"/> and <see cref="DeviceEndSize"/>. <para />
    /// The first array element (0) contains deltas for a ppem size of <see cref="DeviceStartSize"/>, 
    /// while the last element contains deltas for a ppem size of <see cref="DeviceEndSize"/>. <para/>
    /// Only set when <see cref="Format"/> is not set to <see cref="DeviceVariableTableFormat.VariationIndex"/>.</summary>
    public int[] DeltaValues => _deltas;

    int[] _deltas;

    internal DeviceVariationIndexTable(EnhancedBinaryReader reader, Logger log, IFontTable parent, long offset)
        : base(reader, log, parent, offset)
    {
        DeviceStartSize = reader.ReadUInt16();
        DeviceEndSize = reader.ReadUInt16();
        Format = (DeviceVariableTableFormat)reader.ReadUInt16();

        switch (Format)
        {
            case DeviceVariableTableFormat.Local2BitDeltas:
                _deltas = UnpackDeltas(2, reader.ReadUInt16());
                break;

            case DeviceVariableTableFormat.Local4BitDeltas:
                _deltas = UnpackDeltas(4, reader.ReadUInt16());
                break;

            case DeviceVariableTableFormat.Local8BitDeltas:
                _deltas = UnpackDeltas(8, reader.ReadUInt16());
                break;

            case DeviceVariableTableFormat.VariationIndex:
                DeltaSetOuterIndex = DeviceStartSize;
                DeltaSetInnerIndex = DeviceEndSize;

                // Un-set device table-specific properties.
                DeviceStartSize = DeviceEndSize = 0;
                break;
        }
    }

    private int[] UnpackDeltas(int bitsPerValue, ushort packed)
    {
        int valCount = sizeof(ushort) / bitsPerValue;
        int rShift = sizeof(int) - bitsPerValue;
        int[] result = new int[valCount];

        for (int i = 0; i < valCount; i++)
            result[i] = (packed << (16 + (bitsPerValue * i))) >> rShift;

        // ushort bits for 4 packed values: 
        //      0000|0000|0000|0000
        // unpack 4-bit values from ushort
        //int[] vals = new int[4]{
        //    (packed << 16) >> 28,
        //    (packed << 20) >> 28,
        //    (packed << 24) >> 28,
        //    (packed << 28) >> 28,
        //};

        return result;
    }
}
