using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Control value table (CVT).<para/>
    /// See: https://docs.microsoft.com/en-us/typography/opentype/spec/cvt </summary>
    [FontTableTag("cvt")]
    public class Cvt : FontTable
    {
        /// <summary>Gets an array of values referenceable by instructions (such as those in a 'prep' table). </summary>
        public int[] Values { get; private set; }

        internal override void Read(BinaryEndianAgnosticReader reader, TableHeader header, Logger log, FontTableList dependencies)
        {
            uint valueCount = header.Length / 2; //FWORD -- int16 that describes a quantity in font design units. 
            Values = new int[valueCount];
            reader.ReadArrayInt16(Values, (int)valueCount);
        }
    }
}
