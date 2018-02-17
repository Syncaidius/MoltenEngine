using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>Stores font tables by their tag and type. Also stores the headers of unsupported tables.</summary>
    public class FontTableList
    {
        Dictionary<string, FontTable> _byTag;
        Dictionary<Type, FontTable> _byType;
        List<TableHeader> _unsupported;

        // For read-only access.
        IReadOnlyCollection<TableHeader> _unsupportedReadOnly;
        IReadOnlyCollection<FontTable> _tablesReadOnly;

        internal FontTableList()
        {
            _unsupported = new List<TableHeader>();
            _unsupportedReadOnly = _unsupported.AsReadOnly();

            _byTag = new Dictionary<string, FontTable>();
            _byType = new Dictionary<Type, FontTable>();
        }

        internal void AddUnsupported(TableHeader header)
        {
            _unsupported.Add(header);
        }

        internal void Add(FontTable table)
        {
            _byTag.Add(table.Header.Tag, table);
            _byType.Add(table.GetType(), table);
        }

        /// <summary>Retrieves a table from the dictory list, or null if it isn't present.</summary>
        /// <typeparam name="T">The table type.</typeparam>
        /// <returns>A <see cref="FontTable"/> of type T, or null if not present.</returns>
        public T Get<T>() where T : FontTable
        {
            if (_byType.TryGetValue(typeof(T), out FontTable table))
                return table as T;
            else
                return null;
        }

        /// <summary>Retrieves a table from the list, or null if it isn't present.</summary>
        /// <param name="tableTag">The table's tag (e.g. 'hhea, 'maxp').</param>
        /// <returns>A <see cref="FontTable"/>, or null if not present.</returns>
        public FontTable Get(string tableTag)
        {
            if (_byTag.TryGetValue(tableTag, out FontTable table))
                return table;
            else
                return null;
        }

        /// <summary>Gets a read-only list of loaded tables that were loaded from the original font data.</summary>
        public IReadOnlyCollection<FontTable> Tables => _tablesReadOnly;

        /// <summary>Gets a read-only list of headers for tables that were found in the original font data, but not supported.</summary>
        public IReadOnlyCollection<TableHeader> UnsupportedTables => _unsupportedReadOnly;
    }
}
