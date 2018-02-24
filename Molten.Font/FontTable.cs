using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public interface IFontTable
    {
        TableHeader Header { get; }
    }

    public abstract class FontTable : IFontTable
    {
        /// <summary>Gets a list of tags for tables that the parser depends on to parse it's own table type.</summary>
        public string[] Dependencies { get; internal set; }

        public TableHeader Header { get; internal set; }

        internal abstract void Read(BinaryEndianAgnosticReader reader, TableHeader header, Logger log, FontTableList dependencies);
    }
}
