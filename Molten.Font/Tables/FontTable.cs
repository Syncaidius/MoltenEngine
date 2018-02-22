using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public abstract class FontTable
    {
        public TableHeader Header { get; internal set; }
    }

    internal abstract class FontTableParser
    {
        internal abstract FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log, FontTableList dependencies);

        /// <summary>Gets the table's expected tag string (e.g. cmap, CFF, head, name, OS/2).</summary>
        public abstract string TableTag { get; }

        /// <summary>Gets a list of tags for tables that the parser depends on to parse it's own table type.</summary>
        public virtual string[] Dependencies => null;
    }
}
