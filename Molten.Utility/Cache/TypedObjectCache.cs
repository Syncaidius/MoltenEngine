namespace Molten.Cache;

/// <summary>
/// An object cache that is restricted to storing a specific type of object.
/// </summary>
/// <typeparam name="T">The type of object to be stored.</typeparam>
public class TypedObjectCache<T>
    where T : IEquatable<T>
{
    T[] _items;
    int _count = 0;
    Stack<int> _free;

    /// <summary>
    /// Creates a new instance of <see cref="TypedObjectCache{T}"/>.
    /// </summary>
    /// <param name="initialCapacity">The initial capacity of the cache.</param>
    public TypedObjectCache(int initialCapacity = 0)
    {
        _free = new Stack<int>(initialCapacity > 0 ? initialCapacity : 2);
        _items = new T[initialCapacity];
    }

    /// <summary>
    /// Caches the provided object, or disposes it and replaces it with the matching cached object.
    /// <para>Returns false if a match was not found in the cache. 
    /// Returns true if a match was found and the provided object was replaced with a cached one.</para>
    /// </summary>
    /// <param name="obj">The object to cache-check.</param>
    public bool Check(ref T obj)
    {
        // Check cache for a matching object.
        for(int i = 0; i < _items.Length; i++)
        {
            ref T item = ref _items[i];

            if (item.Equals(obj))
            {
                if(obj is IDisposable disposable)
                    disposable.Dispose();

                obj = item;
                return true;
            }
        }

        Add(ref obj);
        return false;
    }

    /// <summary>
    /// Adds the provided <paramref name="obj"/> to the cache without checking for duplicates.
    /// </summary>
    /// <param name="obj">The object to add to the cache.</param>
    public void Add(ref readonly T obj)
    {
        if(_free.Count > 0)
        {
            int index = _free.Pop();
            _items[index] = obj;
        }
        else
        {
            if (_count == _items.Length)
                Array.Resize(ref _items, _items.Length * 2);

            _items[_count++] = obj;
        }
    }

    /// <summary>
    /// Removes an object from the cache. If a match was not found to remove, the method returns false.
    /// </summary>
    /// <param name="obj">The object to remove from the cache.</param>
    /// <returns></returns>
    public bool Remove(ref readonly T obj)
    {
        for(int i = 0; i < _items.Length; i++)
        {
            ref T item = ref _items[i];

            if (item.Equals(obj))
            {
                _items[i] = default;
                _free.Push(i);
                return true;
            }
        }

        return false;
    }
}
