namespace Molten
{
    public interface IMatrix<T> 
        where T : unmanaged
    {
        T[] ToArray();

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the matrix component, depending on the index.</value>
        /// <param name="index">The zero-based index of the component to access.</param>
        /// <returns>The value of the component at the specified index.</returns>
        T this[int index] { get; set; }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the matrix component, depending on the index.</value>
        /// <param name="index">The zero-based index of the component to access.</param>
        /// <returns>The value of the component at the specified index.</returns>
        T this[uint index] { get; set; }

        /// <summary>
        /// Gets or sets the component at the specified row and column index.
        /// </summary>
        /// <value>The value of the matrix component, depending on the index.</value>
        /// <param name="row">The row of the matrix to access.</param>
        /// <param name="column">The column of the matrix to access.</param>
        /// <returns>The value of the component at the specified index.</returns>
        T this[int row, int column] { get; set; }

        /// <summary>
        /// Gets or sets the component at the specified row and column index.
        /// </summary>
        /// <value>The value of the matrix component, depending on the index.</value>
        /// <param name="row">The row of the matrix to access.</param>
        /// <param name="column">The column of the matrix to access.</param>
        /// <returns>The value of the component at the specified index.</returns>

        T this[uint row, uint column] { get; set; }

        /// <summary>
        /// Gets the number of components in the current matrix type.
        /// </summary>
        static readonly int ComponentCount;

        /// <summary>
        /// The number of rows in the current matrix type.
        /// </summary>
        static readonly int RowCount;

        /// <summary>
        /// The number of columns in the current matrix type.
        /// </summary>
        static readonly int ColumnCount;
    }
}
