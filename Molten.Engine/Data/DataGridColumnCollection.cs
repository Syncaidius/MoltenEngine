using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Data
{
    public class DataGridColumnCollection
    {
        List<IDataSet> _columns;

        internal DataGridColumnCollection()
        {
            _columns = new List<IDataSet>();
        }

        public void Add(IDataSet column)
        {
            column.OnCountChanged += Column_OnCountChanged;
            _columns.Add(column);
        }

        private void Column_OnCountChanged(IDataSet set)
        {
            LargestColumnCount = 0;
            for (int i = 0; i < _columns.Count; i++)
                LargestColumnCount = Math.Max(LargestColumnCount, _columns[i].Count);
        }

        public void Remove(IDataSet column)
        {
            int index = _columns.IndexOf(column);
            Remove(index);
        }

        public void Remove(int columnIndex)
        {
            IDataSet column = _columns[columnIndex];
            column.OnCountChanged -= Column_OnCountChanged;

            _columns.RemoveAt(columnIndex);
        }

        /// <summary>
        /// The count of the column with the largest number of values in it.
        /// </summary>
        public int LargestColumnCount { get; private set; }

        /// <summary>
        /// Gets the number of columns.
        /// </summary>
        public int Count => _columns.Count;

        /// <summary>
        /// Gets a column at the given index.
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public IDataSet this[int columnIndex]
        {
            get
            {
                if (columnIndex > _columns.Count)
                    throw new IndexOutOfRangeException("The column index exceeds the number of columns in the data-grid.");

                return _columns[columnIndex];
            }
        }

    }
}
