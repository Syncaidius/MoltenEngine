using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Data
{
    public delegate void DataSetChangeHandler(IDataSet set);

    /// <summary>
    /// Represents a single axis of data.
    /// </summary>
    public interface IDataSet
    {
        /// <summary>
        /// Invoked when the number of items in the dataset has changed.
        /// </summary>
        event DataSetChangeHandler OnCountChanged;

        /// <summary>
        /// Clears the dataset.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets or sets the data-set label.
        /// </summary>
        string Label { get; set; }

        /// <summary>
        /// Gets the number of values in the current <see cref="IDataSet"/>.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the capacity of the current <see cref="IDataSet"/>. 
        /// This is only the number of values the <see cref="IDataSet"/> is capable of storing, not the value count.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// The datatype of the current <see cref="IDataSet"/>.
        /// </summary>
        Type DataType { get; }

        /// <summary>
        /// Gets the color used to draw the graphical representation of the data, or legend key of the current <see cref="IDataSet"/>.
        /// </summary>
        Color KeyColor { get; set; }

        /// <summary>
        /// Gets or sets data at the specified index.
        /// </summary>
        /// <param name="index">The value index.</param>
        /// <returns></returns>
        object this[int index] { get; set; }
    }

    /// <summary>
    /// Represents a typed, single axis of data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataSet<T> : IDataSet
    {
        /// <summary>
        /// Gets or sets data at the specified index.
        /// </summary>
        /// <param name="index">The value index.</param>
        /// <returns></returns>
        new T this[int index] { get; set; }

        /// <summary>
        /// Gets a span of the values stored in the current <see cref="IDataSet{T}"/>.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public Span<T> GetSpan(int startIndex, int count);
    }
}
