using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>The font program table (fpgm).<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/fpgm </summary>
    [FontTableTag("fpgm")]
    public class Fpgm : FontTable
    {
        /// <summary>Gets the table's font program bytecode.</summary>
        public byte[] ByteCode { get; private set; }

        internal override void Read(EnhancedBinaryReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            ByteCode = reader.ReadBytes((int)header.Length);
        }
    }

}
