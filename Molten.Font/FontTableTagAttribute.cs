using System;

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
