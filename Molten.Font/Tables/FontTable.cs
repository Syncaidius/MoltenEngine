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
        internal abstract FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log);

        /// <summary>Reads times in the same format as a 'head' table -- 64 bit times, seconds since 00:00:00, 1-Jan-1904.</summary>
        /// <param name="reader">The reader with which to read the date.</param>
        /// <returns></returns>
        protected DateTime ReadHeadDate(BinaryEndianAgnosticReader reader)
        {
            DateTime baseTime = new DateTime(1904, 1, 1, 0, 0, 0);
            long secondsFromBase = reader.ReadInt64();
            return baseTime + TimeSpan.FromSeconds(secondsFromBase);
        }

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
    }
}
