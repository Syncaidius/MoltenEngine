namespace Molten.Font;

public class TableHeader
{
    /// <summary>A 4-character identifier.</summary>
    public string Tag { get; internal set; }

    /// <summary>The checksum for the table represented by the current <see cref="TableHeader"/>.</summary>
    public uint CheckSum { get; internal set; }

    /// <summary>The offset of the table, from the beginning of a root table's stream.</summary>
    public long StreamOffset { get; internal set; }

    /// <summary>The offset from the beginning of the font file.</summary>
    public long FileOffset { get; internal set; }

    /// <summary>The length of the table that the current <see cref="TableHeader"/> represents.</summary>
    public uint Length { get; internal set; }

    /// <summary>Gets the depth of the table represented by the current <see cref="TableHeader"/>. <para/>
    /// Table depth describes how many levels down/in the hierarchy a table is. A depth of 0 is equivilent to a root font table.</summary>
    public int TableDepth { get; internal set; }

    public override string ToString()
    {
        return $"{Tag} -- {Length} bytes";
    }
}
