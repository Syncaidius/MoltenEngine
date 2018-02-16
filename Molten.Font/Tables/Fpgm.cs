using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>The font program table (fpgm).<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/fpgm </summary>
    public class Fpgm : FontTable
    {
        /// <summary>Gets the table's font program bytecode.</summary>
        public byte[] ByteCode { get; private set; }

        internal class Parser : FontTableParser
        {
            public override string TableTag => "fpgm";

            internal override FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log, DependencyList dependencies)
            {
                Fpgm table = new Fpgm()
                {
                    ByteCode = reader.ReadBytes((int)header.Length),
                };

                return table;
            }
        }
    }

}
