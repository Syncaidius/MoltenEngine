using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics.Font
{
    class TableHeader
    {
        /// <summary>A 4-character identifier.</summary>
        public string Tag;

        /// <summary>The checksum for the table represented by the current <see cref="TableHeader"/>.</summary>
        public uint CheckSum;

        /// <summary>The offset of the table, from the beginning of the font file.</summary>
        public uint Offset;

        /// <summary>The length of the table that the current <see cref="TableHeader"/> represents.</summary>
        public uint Length;

        public override string ToString()
        {
            return $"{Tag} -- {Length} bytes";
        }
    }
}
