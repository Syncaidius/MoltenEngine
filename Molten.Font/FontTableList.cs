namespace Molten.Font;

/// <summary>Stores font tables by their tag and type. Also stores the headers of unsupported tables.</summary>
public class FontTableList
{
    Dictionary<string, FontTable> _byTag;
    Dictionary<Type, FontTable> _byType;
    List<FontTable> _tables;
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
        _tables = new List<FontTable>();
        _tablesReadOnly = _tables.AsReadOnly();
    }

    /// <summary>Drops a loaded font table. Returns true if the table was found and succesfully dropped.</summary>
    /// <param name="tableTag">The tag of the table to be dropped.</param>
    /// <returns></returns>
    public bool Drop(string tableTag)
    {
        if (_byTag.TryGetValue(tableTag, out FontTable table))
        {
            _byTag.Remove(tableTag);
            _byType.Remove(table.GetType());
            _tables.Remove(table);
            return true;
        }

        return false;
    }

    /// <summary>Drops a loaded font table. Returns true if the table was found and succesfully dropped.</summary>
    /// <typeparam name="T">The type of <see cref="FontTable"/> to be dropped.</typeparam>
    /// <returns></returns>
    public bool Drop<T>() where T : FontTable
    {
        Type t = typeof(T);
        if (_byType.TryGetValue(t, out FontTable table))
        {
            _byTag.Remove(table.Header.Tag);
            _byType.Remove(t);
            _tables.Remove(table);
            return true;
        }

        return false;
    }

    internal void AddUnsupported(TableHeader header)
    {
        _unsupported.Add(header);
    }

    internal void Add(FontTable table)
    {
        _byTag.Add(table.Header.Tag, table);
        _byType.Add(table.GetType(), table);
        _tables.Add(table);
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
