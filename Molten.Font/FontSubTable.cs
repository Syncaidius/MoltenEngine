using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public abstract class FontSubTable : IFontTable
    {
        /// <summary>Gets the parent <see cref="FontTable"/>.</summary>
        public IFontTable Parent { get; private set; }

        public TableHeader Header { get; internal set; }

        /// <summary>Creates a new instance of a <see cref="FontSubTable"/></summary>
        /// <param name="reader">A binary reader to read the table.</param>
        /// <param name="log">A logger.</param>
        /// <param name="parent">The parent table.</param>
        /// <param name="offset">The offset in bytes from the start of the parent table.</param>
        internal FontSubTable(BinaryEndianAgnosticReader reader, Logger log, IFontTable parent, long offset)
        {
            Parent = parent;
            Header = new TableHeader()
            {
                Offset = parent.Header.Offset + offset,
                Tag = GetType().Name,
            };
        }
    }
}
