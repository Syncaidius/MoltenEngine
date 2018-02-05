using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Collections
{
    public delegate void WatchableListHandler<T>(WatchableList<T> list, T item);

    public delegate void WatchableListRangeHandler<T>(WatchableList<T> list, IEnumerable<T> items, int itemsStartIndex);

    public class WatchableList<T> : List<T>
    {
        public event WatchableListHandler<T> OnItemAdded;

        public event WatchableListRangeHandler<T> OnRangeAdded;

        public event WatchableListHandler<T> OnItemRemoved;

        public event WatchableListRangeHandler<T> OnRangeRemoved;

        public void Add(T item)
        {
            base.Add(item);
            OnItemAdded?.Invoke(this, item);
        }

        public void AddRange(IEnumerable<T> enumerable)
        {
            ICollection<T> collection = enumerable as ICollection<T>;
            int startIndex = collection != null ? collection.Count : 0;
            base.AddRange(enumerable);
            OnRangeAdded?.Invoke(this, enumerable, startIndex);
        }

        public new bool Remove(T item)
        {
            bool r = base.Remove(item);
            OnItemRemoved?.Invoke(this, item);
            return r;
        }

        public new void RemoveAt(int index)
        {
            T item = base[index];
            base.RemoveAt(index);
            OnItemRemoved?.Invoke(this, item);
        }

        public void RemoveRange(int index, int count, ICollection<T> destination)
        {
            int startIndex = destination.Count;
            int end = index + count;
            for (int i = index; i < end; i++)
                destination.Add(base[i]);

            base.RemoveRange(index, count);
            OnRangeRemoved?.Invoke(this, destination, startIndex);
        }

        public new ICollection<T> RemoveRange(int index, int count)
        {
            List<T> destination = new List<T>();

            int end = index + count;
            for(int i = index; i < end; i++)
                destination.Add(base[i]);

            base.RemoveRange(index, count);
            OnRangeRemoved?.Invoke(this, destination, 0);

            return destination;
        }

        public new T this[int index]
        {
            get => base[index];
            set
            {
                T old = base[index];
                if (!value.Equals(old))
                {
                    base[index] = value;
                    OnItemRemoved(this, old);
                    OnItemAdded(this, value);
                }
            }
        }
    }
}
