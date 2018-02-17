using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>A loaded version of a font file.</summary>
    public class FontFile
    {
        FontTableList _tables;

        internal FontFile()
        {
            _tables = new FontTableList();
        }

        internal void Build()
        {
            // TODO Build the font (glyphs, metadata, etc) using data from provided tables.
        }

        /// <summary>Gets the font's table list, which can be used to access any loaded tables, or the headers of tables that were not supported.</summary>
        public FontTableList Tables => _tables;
    }
}
