using Molten.IO;

namespace Molten.Font
{
    /// <summary>Kerning table (maxp).<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/kern </summary>
    [FontTableTag("kern")]
    public class Kern : FontTable
    {
        public ushort Version { get; private set; }

        public KerningTable[] Tables { get; private set; }

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            /* TODO NOTE: Previous versions of the 'kern' table defined both the version and nTables fields in the header as UInt16 values and not UInt32 values. 
             * Use of the older format on OS X is discouraged (although AAT can sense an old kerning table and still make correct use of it). 
             * Microsoft Windows still uses the older format for the 'kern' table and will not recognize the newer one. 
             * Fonts targeted for OS X only should use the new format; fonts targeted for both OS X and Windows should use the old format.
             * See: https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6kern.html
             */
            Version = reader.ReadUInt16();
            ushort numTables = reader.ReadUInt16();
            Tables = new KerningTable[numTables];
            for (int i = 0; i < numTables; i++)
                Tables[i] = new KerningTable(reader, log, this, FontUtil.GetOffset(Header.StreamOffset, reader.Position));
        }
    }


}
