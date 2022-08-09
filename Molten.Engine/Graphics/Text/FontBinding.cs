using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Font;

namespace Molten.Graphics
{
    internal class FontBinding
    {
        public FontFile File { get; }

        public FontLookupTable Lookup { get; }

        public FontBinding(FontFile file, FontLookupTable lookup)
        {
            File = file;
            Lookup = lookup;
        }
    }
}
