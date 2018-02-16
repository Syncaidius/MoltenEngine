using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    /// <summary>A helper class for managing font table dependencies (i.e. tables that depend on other tables for certain information).</summary>
    internal class DependencyList
    {
        Dictionary<string, FontTable> _byTag = new Dictionary<string, FontTable>();
        Dictionary<Type, FontTable> _byType = new Dictionary<Type, FontTable>();

        internal void Add(FontTable table)
        {
            _byTag.Add(table.Header.Tag, table);
            _byType.Add(table.GetType(), table);
        }

        /// <summary>Retrieves a table from the dependency list, or null if it isn't present.</summary>
        /// <typeparam name="T">The table type.</typeparam>
        /// <returns>A <see cref="FontTable"/> of type T, or null if not present.</returns>
        public T Get<T>() where T : FontTable
        {
            if (_byType.TryGetValue(typeof(T), out FontTable table))
                return table as T;
            else
                return null;
        }

        /// <summary>Retrieves a table from the dependency list, or null if it isn't present.</summary>
        /// <param name="tableTag">The dependency's table tag (e.g. 'hhea, 'maxp').</param>
        /// <returns>A <see cref="FontTable"/>, or null if not present.</returns>
        public FontTable Get(string tableTag)
        {
            if (_byTag.TryGetValue(tableTag, out FontTable table))
                return table;
            else
                return null;
        }

        /// <summary>Gets the number of dependency tables in the list.</summary>
        public int Count => _byTag.Count;
    }
}
