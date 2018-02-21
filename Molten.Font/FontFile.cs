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
        FontFlags _flags;
        
        internal FontFile(FontTableList tables)
        {
            _tables = tables;
            _flags = FontFlags.Invalid;
        }

        /// <summary>
        /// (Re)Builds the font based on it's set of available tables. <para/>
        /// You can drop any tables you do not want the font to be built with via <see cref="Tables"/>.<para/>
        /// If the font has already been built previously, calling this again will cause the font to be rebuilt from the tables present in <see cref="Tables"/>.
        /// </summary>
        public void Build()
        {
            // If the flags are invalid, we cannot make a usable FontFile instance.
            _flags = FontValidator.Validate(_tables);
            if (_flags == FontFlags.Invalid)
                return;

            // TODO Build the font (glyphs, metadata, etc) using data from provided tables.
        }

        /// <summary>Returns true if the current <see cref="Flags"/> contains the specified flag value.</summary>
        /// <param name="flag">The flags to test.</param>
        /// <returns>A boolean value.</returns>
        public bool HasFlag(FontFlags flag)
        {
            return (_flags & flag) == flag;
        }

        /// <summary>Gets the font's table list, which can be used to access any loaded tables, or the headers of tables that were not supported.<para/>
        /// You can also use the table list to drop loaded tables. This may be useful if you wish to reduce the font's footprint after it has been built.</summary>
        public FontTableList Tables => _tables;

        /// <summary>
        /// Gets a flags value containing information about the current <see cref="FontFile"/>.
        /// </summary>
        public FontFlags Flags => _flags;
    }
}
