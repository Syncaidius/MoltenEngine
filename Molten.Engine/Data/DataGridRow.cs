namespace Molten.Data;

public class DataGridRow
{
    DataGrid _grid;

    internal DataGridRow(DataGrid grid)
    {
        _grid = grid;
    }

    public int RowID { get; internal set; }

    /// <summary>
    /// Gets or sets a value in the current <see cref="DataGridRow"/>, at the specified column index.
    /// </summary>
    /// <param name="columnIndex"></param>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public object this[int columnIndex]
    {
        get
        {
            IDataSet column = _grid.Columns[columnIndex];
            return RowID < column.Count ? column[RowID] : null;
        }
        set
        {
            if (columnIndex > _grid.Columns.Count)
                throw new IndexOutOfRangeException("The column index exceeds the number of columns in the data-grid.");

            _grid.Columns[columnIndex][RowID] = value;
        }
    }
}
