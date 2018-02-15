using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Control value program table .<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/prep </summary>
    public class Prep : FontTable
    {
        /// <summary>
        /// Gets a set of instructions executed whenever point size or font or transformation change. n is the number of uint8 items that fit in the size of the table.
        /// </summary>
        public byte[] Instructions { get; private set; }

        internal class Parser : FontTableParser
        {
            public override string TableTag => "prep";

            internal override FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log)
            {
                Prep table = new Prep()
                {
                    Instructions = reader.ReadBytes((int)header.Length),
                };

                // TODO expand upon this so that the instructions and their values can be accessed easily (instead of just a byte array).

                return table;
            }
        }
    }

}
