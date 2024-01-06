namespace Molten.Font;

/// <summary>The expected format of the index-to-location (loca) table, if present.</summary>
public enum FontLocaFormat
{
    UnsignedInt16 = 0,

    UnsignedInt32 = 1,
}
