using Molten.IO;

namespace Molten.Font;

/// <summary>FontForge Time-stamp table. See: https://fontforge.github.io/non-standard.html#FFTM</summary>
[FontTableTag("FFTM")]
public class FFTM : FontTable
{
    public uint Version { get; internal set; }

    /// <summary>Gets the time-stamp of the FontForge sources used to create the font.</summary>
    public DateTime SourceTimeStamp { get; internal set; }

    /// <summary>Gets the creation date of the font. 
    /// This is not the creation date of the tt/ot file, but the date the sfd file was created. 
    /// (not always accurate).</summary>
    public DateTime CreationDate { get; internal set; }

    /// <summary>Gets the date that the font was last modified. 
    /// This is not the modification date of the file, but the time a glyph, etc. was last changed in the font database. 
    /// (not always accurate)</summary>
    public DateTime LastModified { get; internal set; }

    internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
    {
        Version = reader.ReadUInt32();
        SourceTimeStamp = FontUtil.FromLongDate(reader.ReadInt64());
        CreationDate = FontUtil.FromLongDate(reader.ReadInt64());
        LastModified = FontUtil.FromLongDate(reader.ReadInt64());
    }
}
