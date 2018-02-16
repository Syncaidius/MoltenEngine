using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Control value table (CVT).<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/cvt </summary>
    public class Cvt : FontTable
    {
        /// <summary>Gets an array of values referenceable by instructions (such as those in a 'prep' table). </summary>
        public short[] Values { get; private set; }

        internal class Parser : FontTableParser
        {
            public override string TableTag => "cvt";

            internal override FontTable Parse(BinaryEndianAgnosticReader reader, TableHeader header, Logger log)
            {
                uint valueCount = header.Length / 2; //FWORD -- int16 that describes a quantity in font design units. 
                Cvt table = new Cvt()
                {
                    Values = new short[valueCount]
                };

                for (int i = 0; i < valueCount; i++)
                    table.Values[i] = reader.ReadInt16();
                return table;
            }
        }
    }

}
