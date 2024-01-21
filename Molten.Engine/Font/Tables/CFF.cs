using Molten.IO;

namespace Molten.Font;

/// <summary>CFF — Compact Font Format table.<para/>
/// See: http://wwwimages.adobe.com/www.adobe.com/content/dam/acom/en/devnet/font/pdfs/5176.CFF.pdf </summary>
[FontTableTag("CFF", "maxp")]
public class CFF : FontTable
{
    const uint STANDARD_ID_COUNT = 390; // See Appendix A -- SID/Name count (Standard ID).

    const uint MAX_SUBR_NESTING = 10;

    public byte MajorVersion { get; private set; }

    public byte MinorVersion { get; private set; }

    public string FontName { get; private set; }

    internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
    {
        Maxp maxp = dependencies.Get<Maxp>();
        ushort numGlyphs = maxp.NumGlyphs;

        // Read header
        MajorVersion = reader.ReadByte();
        MinorVersion = reader.ReadByte();

        byte headerSize = reader.ReadByte();
        byte offSize = reader.ReadByte();


        if (MajorVersion == 1 && MinorVersion == 0)
        {
            CFFIndexTable nameIndex = new CFFIndexTable(reader, log, this, headerSize);
            CFFIndexTable topDictIndex = new CFFIndexTable(reader, log, this, nameIndex.OffsetToNextBlock);
            CFFIndexTable stringIndex = new CFFIndexTable(reader, log, this, topDictIndex.OffsetToNextBlock);
            CFFIndexTable globalSubStrIndex = new CFFIndexTable(reader, log, this, stringIndex.OffsetToNextBlock);

            uint sidMax = (uint)stringIndex.Length + STANDARD_ID_COUNT;
            ParseNameData(reader, nameIndex);
        }
        else
        {
            log.Debug($"[CFF] Unsupported CFF version {MajorVersion}.{MinorVersion}");
        }
    }

    private void ParseNameData(EnhancedBinaryReader reader, CFFIndexTable index)
    {
        for (int i = 0; i < index.Objects.Length; i++)
        {
            SetLocalOffset(reader, index.Objects[i].Offset);
            FontName = reader.ReadString((int)index.Objects[i].DataSize);
        }
    }

    private enum DictDataType
    {
        TopLevel = 0,

        FdArray = 1,
    }

    private enum DictOperandType
    {
        Integer = 0,

        Real = 1,

        Operator = 2,
    }

    private enum FontFormat
    {
        Uknown = 0,

        CID_Keyed = 1,

        Other = 2, // Including synthetic fonts.
    }
}
