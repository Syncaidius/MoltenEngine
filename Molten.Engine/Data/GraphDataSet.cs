namespace Molten.Data
{
    /// <summary>
    /// A dataset with a fixed capacity. Adding new values once the dataset is full will replace the oldest value, when added via <see cref="Add(T)"/>.
    /// </summary>
    /// <typeparam name="T">The type of data to store in the dataset.</typeparam>
    public class GraphDataSet : DataSet<double>
    {
        /// <summary>
        /// Gets the index that will be used to store the next value that is added with <see cref="Add(double)"/>.
        /// </summary>
        public int NextValueIndex { get; protected set; }

        /// <summary>Gets the lowest value in the current <see cref="GraphDataSet"/>.</summary>
        public double LowestValue { get; protected set; }

        /// <summary>Gets the highest value in the current <see cref="GraphDataSet"/>.</summary>
        public double HighestValue { get; protected set; }

        /// <summary>
        /// Gets the mean average value of all values in the current <see cref="GraphDataSet"/>.
        /// </summary>
        public double AverageValue { get; protected set; }

        bool _isDirty = true;

        public override void Clear()
        {
            base.Clear();
            NextValueIndex = 0;
        }

        public GraphDataSet(int maxValues) :
            base(maxValues)
        {
            _isDirty = true;
        }

        public GraphDataSet(double[] values) :
            base(values)
        {
            _isDirty = true;
        }

        public GraphDataSet(double[] values, int startIndex, int count) : 
            base(values, startIndex, count)
        {
            _isDirty = true;
        }

        /// <summary>
        /// Plots a value in the current <see cref="GraphDataSet"/>.
        /// </summary>
        /// <param name="value">The value to plot.</param>
        public void Plot(double value)
        {
            if (NextValueIndex == Values.Length)
                NextValueIndex = 0;

            if (Count < Values.Length)
            {
                Values[NextValueIndex++] = value;
                Count++;
            }
            else
            {
                Values[NextValueIndex] = value;
                NextValueIndex++;
            }

            _isDirty = true;
        }

        /// <summary>
        /// Updates the <see cref="LowestValue"/>, <see cref="HighestValue"/> and <see cref="AverageValue"/> values,
        /// based on the data in the current <see cref="GraphDataSet"/>.
        /// </summary>
        public void Calculate()
        {
            if (!_isDirty)
                return;

            AverageValue = 0;
            LowestValue = double.MaxValue;
            HighestValue = double.MinValue;

            for(int i = 0; i < Count; i++)
            {
                double val = Values[i];
                if (val < LowestValue)
                    LowestValue = val;

                if (val > HighestValue)
                    HighestValue = val;

                AverageValue += val;
            }

            AverageValue /= Count;
            _isDirty = false;
        }

        /// <summary>
        /// Gets a value from the current <see cref="DataSet{T}"/>
        /// </summary>
        /// <param name="index">The value index.</param>
        /// <returns></returns>
        public override double this[int index]
        {
            get => Values[index];
            set
            {
                if (index >= Values.Length)
                {
                    if (NextValueIndex < Values.Length)
                    {
                        int p1Len = Values.Length - NextValueIndex;
                        int p2Len = NextValueIndex;

                        // We need to join the 2 out-of-order segments back into one sequential list of data,
                        // to prevent breaking the rolling data circle.
                        double[] newValues = new double[index + 100];
                        Array.Copy(Values, NextValueIndex, newValues, 0, p1Len);
                        Array.Copy(Values, 0, newValues, p1Len, p2Len);
                        NextValueIndex = Values.Length;
                        Values = newValues;
                    }
                    else
                    {
                        Array.Resize(ref Values, index + 100);
                    }

                    RaiseCountChangedEvent();
                }

                Values[index] = value;
                _isDirty = true;
            }
        }
    }
}
