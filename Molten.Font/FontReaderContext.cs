using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Font
{
    internal class FontReaderContext
    {
        struct TableEntry
        {
            public long StartPos;
            public FontTable Table;
            public string IndentString;
        }

        int _depth;
        EnhancedBinaryReader _reader;
        TableEntry[] _entryStack;
        int _stackPos;
        Logger _log;

        TableEntry _curEntry;
        MainFontTable _rootTable;

        public FontReaderContext(EnhancedBinaryReader reader, Logger log, MainFontTable rootTable)
        {
            _rootTable = rootTable;
            _reader = reader;
            _entryStack = new TableEntry[10];
            _curEntry = new TableEntry()
            {
                StartPos = 0,
                Table = rootTable,
                IndentString = "",
            };
        }

        public long GetStartPos()
        {
            return _curEntry.StartPos;
        }

        private void Push(FontTable table, uint offset)
        {
            if(_stackPos >= _entryStack.Length)
            {
                TableEntry[] newArray = new TableEntry[_entryStack.Length * 2];
                Array.Copy(_entryStack, 0, newArray, 0, _stackPos);
                _entryStack = newArray;
            }

            _entryStack[_stackPos++] = _curEntry;

            _curEntry = new TableEntry()
            {
                StartPos = _curEntry.StartPos + offset,
                Table = table,
                IndentString = new string('\t', _stackPos),
            };
        }

        private void Pop()
        {
            _curEntry = _entryStack[--_stackPos];
        }

        /// <summary>
        /// Reads a sub-table from the current stream position.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ReadSubTable<T>() where T : FontSubTable, new()
        {
            FontTable parent = _curEntry.Table;
            T table = new T();
            Push(table, 0);
            _curEntry.StartPos = _reader.Position;
            _reader.Position = _curEntry.StartPos;
            table.Read(_reader, this, parent);
            Pop();

            return table;
        }

        public T ReadSubTable<T>(uint offset) where T : FontSubTable, new()
        {
            FontTable parent = _curEntry.Table;
            T table = new T();

            Push(table, offset);
            _reader.Position = _curEntry.StartPos;
            table.Read(_reader, this, parent);
            Pop();

            return table;
        }

        /// <summary>
        /// Reads a list of items from the font file. The data must be contiguous with no gaps.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count"></param>
        /// <param name="offset">An offset to where the list of items is located.</param>
        /// <param name="instantiationCallback">A callback which is invoked to read each item from the stream.</param>
        /// <returns></returns>
        public T[] ReadList<T>(int count, ushort offset, Func<EnhancedBinaryReader, T> instantiationCallback) where T : new()
        {
            T[] result = new T[count];
            for (int i = 0; i < count; i++)
                result[i] = instantiationCallback.Invoke(_reader);

            return result;
        }

        /// <summary>
        /// Writes a debug message into the log.
        /// </summary>
        /// <param name="msg"></param>
        [Conditional("DEBUG")]
        public void WriteLine(string msg)
        {
            _log.WriteDebugLine($"[{_rootTable.Header.Tag}] {_curEntry.IndentString}{msg}");
        }

        /// <summary>
        /// Writes a warning into the log.
        /// </summary>
        /// <param name="msg"></param>
        public void WriteWarning(string msg)
        {
            _log.WriteWarning($"[{_rootTable.Header.Tag}] {_curEntry.IndentString}{msg}");
        }
    }
}
