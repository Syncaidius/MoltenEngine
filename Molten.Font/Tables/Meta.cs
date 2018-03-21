using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Index-to-location table.<para/>
    /// The metadata table contains various metadata values for the font. Different categories of metadata are identified by four-character tags. <para/>
    /// Values for different categories can be either binary or text. <para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/meta </summary>
    [FontTableTag("meta")]
    public class Meta : FontTable
    {
        /// <summary>
        /// Gets the version number of the metadata table — set to 1.
        /// </summary>
        public uint Version { get; private set; }

        /// <summary>
        /// Gets the flags value. Currently unused; set to 0.
        /// </summary>
        public uint Flags { get; private set; }

        /// <summary>
        /// Gets an array of <see cref="DataMapBase"/> containing metadata.
        /// </summary>
        public DataMapBase[] DataMaps { get; private set; }

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            Version = reader.ReadUInt32();
            Flags = reader.ReadUInt32();
            uint reserved = reader.ReadUInt32();
            uint dataMapsCount = reader.ReadUInt32();

            // Gather records for data map tables.
            DataMapRecord[] records = new DataMapRecord[dataMapsCount];
            for(int i = 0; i < dataMapsCount; i++)
            {
                records[i] = new DataMapRecord()
                {
                    Tag = FontUtil.ReadTag(reader),
                    Offset = reader.ReadUInt32(),
                    DataLength = reader.ReadUInt32(),
                };
            }

            // Load data map tables based on record info.
            DataMaps = new DataMapBase[dataMapsCount];
            for (int i = 0; i < dataMapsCount; i++)
            {
                DataMapRecord record = records[i];
                DataMapBase map;

                switch (record.Tag)
                {
                    default:
                        map = new RawDataMap(reader, log, this, record);
                        break;

                    case "slng":
                    case "dlng":
                        map = new AsciiDataMap(reader, log, this, record);
                        break;
                }

                map.Tag = record.Tag;
                map.DataLength = record.DataLength;
                DataMaps[i] = map;
            }
        }
    }

    internal struct DataMapRecord
    {
        public string Tag;

        public uint Offset;

        public uint DataLength;
    }

    public abstract class DataMapBase : FontSubTable
    {
        internal DataMapBase(EnhancedBinaryReader reader, Logger log, IFontTable parent, DataMapRecord record) : 
            base(reader, log, parent, record.Offset)
        {

        }

        /// <summary>
        /// Gets the data-map tag  indicating the type of metadata stored in the table.
        /// </summary>
        public string Tag { get; internal set; }

        /// <summary>
        /// Gets the length of the table's data, in bytes.
        /// </summary>
        public uint DataLength { get; internal set; }

        public override string ToString()
        {
            return $"{base.ToString()} -- {Tag}";
        }
    }

    public class RawDataMap : DataMapBase
    {
        internal RawDataMap(EnhancedBinaryReader reader, Logger log, IFontTable parent, DataMapRecord record) : 
            base(reader, log, parent, record)
        {
            RawData = reader.ReadBytes((int)record.DataLength);
        }

        /// <summary>
        /// Gets the raw data stored in the table.
        /// </summary>
        public byte[] RawData { get; private set; }
    }

    /// <summary>
    /// A data-map table containing ASCII text.<para/>
    /// </summary>
    public class AsciiDataMap : DataMapBase
    {
        internal AsciiDataMap(EnhancedBinaryReader reader, Logger log, IFontTable parent, DataMapRecord record) : 
            base(reader, log, parent, record)
        {
            Data = reader.ReadString((int)record.DataLength);
        }

        /// <summary>
        /// Gets the string data stored in the table.
        /// </summary>
        public string Data { get; private set; }
    }
}
