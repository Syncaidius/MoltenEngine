using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    public class FontTableTagAttribute : Attribute
    {
        public FontTableTagAttribute(string tag, params string[] dependencies)
        {
            Tag = tag;
            Dependencies = dependencies ?? new string[0];
        }

        public string Tag { get; private set; }

        public string[] Dependencies { get; private set; }
    }
}
