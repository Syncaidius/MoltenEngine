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
        Dictionary<string, FontTable> _tables;
        
        internal FontFile()
        {
            _tables = new Dictionary<string, FontTable>();
        }

        /// <summary>Gets a font table that was loaded with the font. Returns null if the table does not exist.<para/>
        /// FontTables provide access to the underlying font data which may be useful in advanced use cases.</summary>
        /// <param name="name">The name of the table. Case-sensitive. Leading and trailing spaces are not required.</param>
        /// <returns>A font table, or null if not found.</returns>
        public FontTable this[string name]
        {
            internal set
            {
                _tables[name] = value;
            }

            get
            {
                if (_tables.TryGetValue(name, out FontTable table))
                    return table;
                else
                    return null;
            }
        }
    }
}
