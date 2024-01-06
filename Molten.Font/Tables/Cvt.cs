using Molten.IO;

namespace Molten.Font;

/// <summary>Control value table (CVT).<para/>
/// See: https://docs.microsoft.com/en-us/typography/opentype/spec/cvt </summary>
[FontTableTag("cvt")]
public class Cvt : FontTable
{
    /// <summary>Gets an array of values referenceable by instructions (such as those in a 'prep' table). </summary>
    public int[] Values { get; private set; }

    internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
    {
        uint valueCount = header.Length / 2;
        short[] shortValues = reader.ReadArray<short>((int)valueCount);
        Values = Array.ConvertAll(shortValues, item => (int)item);
    }
}
