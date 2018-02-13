using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Font
{
    public class TableHeader
    {
        /// <summary>A 4-character identifier.</summary>
        public string Tag { get; internal set; }

        /// <summary>The checksum for the table represented by the current <see cref="TableHeader"/>.</summary>
        public uint CheckSum { get; internal set; }

        /// <summary>The offset of the table, from the beginning of the font file.</summary>
        public uint Offset { get; internal set; }

        /// <summary>The length of the table that the current <see cref="TableHeader"/> represents.</summary>
        public uint Length { get; internal set; }

        public override string ToString()
        {
            return $"{Tag} -- {Length} bytes";
        }
    }
}
