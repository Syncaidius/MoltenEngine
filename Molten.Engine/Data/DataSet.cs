using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Molten.Data
{    
    /// <summary>
    /// A dataset with a fixed capacity. Adding new values once the dataset is full will replace the oldest value, when added via <see cref="Add(T)"/>.
    /// </summary>
    /// <typeparam name="T">The type of data to store in the dataset.</typeparam>
    public abstract class DataSet<T> : IDataSet<T>
    {
        [JsonProperty]
        protected T[] Values;

        public event DataSetChangeHandler OnCountChanged;

        [JsonProperty]
        public Color KeyColor { get; set; } = Color.White;

        [JsonProperty]
        public string Label { get; set; }

        /// <summary>
        /// Gets the number of values in the current <see cref="DataSet{T}"/>
        /// </summary>
        [JsonProperty]
        public int Count { get; protected set; }

        /// <summary>
        /// Gets the capacity of the current <see cref="DataSet{T}"/>
        /// </summary>
        public int Capacity => Values.Length;

        public Type DataType => typeof(T);

        public DataSet(int maxValues)
        {
            Values = new T[maxValues];
        }

        public DataSet(double[] values) 
        {
            Values = new T[values.Length];
            Array.Copy(values, 0, Values, 0, values.Length);
        }

        public DataSet(double[] values, int startIndex, int count) 
        {
            Values = new T[count];
            Array.Copy(values, startIndex, Values, 0, count);
        }

        public Span<T> GetSpan(int startIndex, int count)
        {
            return new Span<T>(Values, startIndex, count);
        }

        public virtual void Clear()
        {
            Count = 0;
        }

        protected void RaiseCountChangedEvent()
        {
            OnCountChanged?.Invoke(this);
        }

        /// <summary>
        /// Gets a value from the current <see cref="DataSet{T}"/>
        /// </summary>
        /// <param name="index">The value index.</param>
        /// <returns></returns>
        public virtual T this[int index]
        {
            get => Values[index];
            set
            {
                if (index >= Values.Length)
                {
                    Array.Resize(ref Values, index + 100);
                    OnCountChanged?.Invoke(this);
                }

                Values[index] = value;
            }
        }

        object IDataSet.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }
    }
}
