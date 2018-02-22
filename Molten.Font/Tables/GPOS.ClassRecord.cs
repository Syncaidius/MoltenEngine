using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public partial class GPOS
    {
        public class Class1Record
        {
            public Class2Record[] Records { get; private set; }

            internal Class1Record(BinaryEndianAgnosticReader reader, ushort class2Count, ValueFormat format1, ValueFormat format2)
            {
                Records = new Class2Record[class2Count];
                for (int i = 0; i < class2Count; i++)
                {
                    Records[i] = new Class2Record()
                    {
                        Record1 = new ValueRecord(reader, format1),
                        Record2 = new ValueRecord(reader, format2),
                    };
                }
            }
        }

        public class Class2Record
        {
            /// <summary>
            /// Gets the positioning data for the first glyph.
            /// </summary>
            public ValueRecord Record1 { get; internal set; }

            /// <summary>
            /// Gets the positioning data for the second glyph.
            /// </summary>
            public ValueRecord Record2 { get; internal set; }
        }
    }
}
