using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Naming (name) table .<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/name </summary>
    [FontTableTag("name")]
    public class Name : FontTable
    {
        public ushort Format { get; private set; }

        /// <summary>
        /// Gets an array of <see cref="NameRecord"/>. Each record contains a string for a specific naming type/ID and platform (e.g. author, copyright, trademark, etc).
        /// </summary>
        public NameRecord[] Records { get; private set; }

        public string[] LanguageTags { get; private set; }

        /// <summary>
        /// Returns a string matching the provided type and platform, or <see cref="String.Empty"/> if not present
        /// </summary>
        /// <param name="type">The type of string to return.</param>
        /// <param name="platform">The platform for which to retrieve the string.</param>
        /// <returns></returns>
        public string GetNameString(FontNameType type, FontPlatform platform)
        {
            // TODO take into account the language.
            foreach(NameRecord record in Records)
            {
                if (record.NameID == type && record.Platform == platform)
                    return record.Value;
            }

            return string.Empty;
        }
        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            Format = reader.ReadUInt16();

            ushort nameCount = reader.ReadUInt16();
            ushort stringOffset = reader.ReadUInt16();
            long stringStorageOffset = header.ReadOffset + stringOffset;

            long returnPos = reader.Position;
            int stringDataLen = (int)(header.Length - stringOffset);
            long endPos = stringStorageOffset + stringDataLen;

            // TODO Review this later for possible improvements
            // Read the string data into a separate memory stream for quicker access
            reader.Position = stringStorageOffset;
            byte[] stringStorageData = reader.ReadBytes(stringDataLen);
            reader.Position = returnPos;

            using (MemoryStream stringStream = new MemoryStream(stringStorageData))
            {
                using (BinaryReader stringStorageReader = new BinaryReader(stringStream))
                {
                    // Read name records. Present in both format 0 and 1.
                    Records = new NameRecord[nameCount];
                    for (int i = 0; i < nameCount; i++)
                    {
                        NameRecord record = new NameRecord()
                        {
                            Platform = (FontPlatform)reader.ReadUInt16(),
                            PlatformEncodingID = reader.ReadUInt16(),
                            LanguageID = reader.ReadUInt16(),
                            NameID = (FontNameType)reader.ReadUInt16(),
                        };

                        ushort length = reader.ReadUInt16();
                        ushort offset = reader.ReadUInt16();

                        // Jump to string in string storage.
                        stringStorageReader.BaseStream.Position = offset;
                        Encoding encoding = FontLookup.GetEncoding(record.Platform, record.PlatformEncodingID);
                        record.Value = encoding.GetString(stringStorageReader.ReadBytes(length)); // TODO use platform-specific encoding if needed/possible.
                        Records[i] = record;
                    }

                    // Read language tag records. Present only in format 1.
                    if (Format == 1)
                    {
                        ushort langTagCount = reader.ReadUInt16();
                        LanguageTags = new string[langTagCount];
                        for (int i = 0; i < langTagCount; i++)
                        {
                            ushort length = reader.ReadUInt16();
                            ushort offset = reader.ReadUInt16();

                            // Language-tag strings stored in the Naming table must be encoded in UTF-16BE (Big Endian). 
                            LanguageTags[i] = Encoding.BigEndianUnicode.GetString(stringStorageReader.ReadBytes(length)).Replace("\0", "");
                        }
                    }
                    else
                    {
                        LanguageTags = new string[0];
                    }
                }
            }

            // Jump to end of table so we're correctly aligned.
            // Not required but, it helps with debugging...
            reader.Position = endPos;
        }

        public class NameRecord
        {
            public FontPlatform Platform { get; internal set; }

            /// <summary>Gets the platform-specific encoding ID. <para/>
            /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/name#enc3 <para/>
            /// See Also: https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6name.html</summary>
            public ushort PlatformEncodingID { get; internal set; }

            public ushort LanguageID { get; internal set; }

            public FontNameType NameID { get; internal set; }

            public string Value { get; internal set; }

            public override string ToString()
            {
                return $"{NameID}: {Value}";
            }
        }
    }
}
