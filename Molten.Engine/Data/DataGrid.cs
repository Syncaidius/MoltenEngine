using Newtonsoft.Json;

namespace Molten.Data
{
    public delegate void DataGridHandler(DataGrid grid, IDataSet set);

    public class DataGrid
    {
        [JsonProperty]
        Dictionary<int, DataGridRow> _rows;

        public DataGrid()
        {
            _rows = new Dictionary<int, DataGridRow>();
            Columns = new DataGridColumnCollection();
        }

        public DataGridColumnCollection Columns { get; }

        /// <summary>
        /// Gets a row from the current <see cref="DataGrid"/>.
        /// </summary>
        /// <param name="rowIndex">The index of the row to retrieve.</param>
        /// <returns></returns>
        public DataGridRow this[int rowIndex]
        {
            get
            {
                if(!_rows.TryGetValue(rowIndex, out DataGridRow row))
                {
                    row = new DataGridRow(this);
                    _rows.Add(rowIndex, row);
                }

                return row;
            }
        }

        /// <summary>
        /// Gets a value at the specified row and column index.
        /// </summary>
        /// <param name="rowIndex">The row index of the value.</param>
        /// <param name="columnIndex">The column index of the value.</param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public object this[int rowIndex, int columnIndex]
        {
            get
            {
                if (rowIndex >= Columns.LargestColumnCount)
                    throw new IndexOutOfRangeException("The row index exceeds the column value count.");

                return Columns[columnIndex][rowIndex];
            }
        }
    }
}
