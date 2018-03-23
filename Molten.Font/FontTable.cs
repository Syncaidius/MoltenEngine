using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public interface IFontTable
    {
        /// <summary>
        /// Gets the current table's header.
        /// </summary>
        TableHeader Header { get; }
    }

    public abstract class FontTable : IFontTable
    {
        /// <summary>Gets a list of tags for tables that the parser depends on to parse it's own table type.</summary>
        public string[] Dependencies { get; internal set; }

        /// <summary>
        /// Gets the current table's header.
        /// </summary>
        public TableHeader Header { get; internal set; }

        /// <summary>Populates the table from a stream using as <see cref="EnhancedBinaryReader"/>.</summary>
        /// <param name="reader"></param>
        /// <param name="header"></param>
        /// <param name="log"></param>
        /// <param name="dependencies"></param>
        internal abstract void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies);

        protected uint GetLocalOffset(EnhancedBinaryReader reader)
        {
            return (uint)(reader.Position - Header.StreamOffset);
        }

        protected void SetLocalOffset(EnhancedBinaryReader reader, uint offset)
        {
            reader.Position = Header.StreamOffset + offset;
        }
    }
}
