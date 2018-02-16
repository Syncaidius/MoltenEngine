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
        internal abstract FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log, DependencyList dependencies);

        /// <summary>Attempts to read a sub-table if the provided subTableOffset is greater than 0.</summary>
        /// <param name="reader"></param>
        /// <param name="log"></param>
        /// <param name="subTableName"></param>
        /// <param name="subTableOffset"></param>
        /// <param name="parentHeader"></param>
        /// <param name="callback"></param>
        protected void ReadSubTable(
            BinaryEndianAgnosticReader reader,
            Logger log,
            string subTableName, 
            long subTableOffset, 
            TableHeader parentHeader, 
            Action<long> callback)
        {
            if (subTableOffset == 0)
                return;

            log.WriteDebugLine($"[{parentHeader.Tag}] Reading sub-table '{subTableName}' -- Local pos: {subTableOffset}/{parentHeader.Length}");

            long subPos = parentHeader.Offset + subTableOffset;
            reader.Position = subPos;
            callback(subPos);
        }

        /// <summary>Gets the table's expected tag string (e.g. cmap, CFF, head, name, OS/2).</summary>
        public abstract string TableTag { get; }

        /// <summary>Gets a list of tags for tables that the parser depends on to parse it's own table type.</summary>
        public virtual string[] Dependencies => null;
    }
}
