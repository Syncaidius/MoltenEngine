namespace Molten.Cache;

/// <summary>
/// An object cache where another type of object is used as the key.
/// </summary>
public class KeyedObjectCache<K, V>
    where K : IEquatable<K>
{
    private struct Pair
    {
        public K Key;

        public V Value;
    }

    Pair[] _pairs;
    int _count = 0;
    Stack<int> _free;

    /// <summary>
    /// Creates a new instance of <see cref="TypedObjectCache{T}"/>.
    /// </summary>
    /// <param name="initialCapacity">The initial capacity of the cache.</param>
    public KeyedObjectCache(int initialCapacity = 1)
    {
        initialCapacity = Math.Max(1, initialCapacity);
        _free = new Stack<int>(initialCapacity > 0 ? initialCapacity : 2);
        _pairs = new Pair[initialCapacity];
    }

    /// <summary>
    /// Caches the provided object, or disposes it and replaces it with the matching cached object.
    /// <para>Returns false if a match was not found in the cache. 
    /// Returns true if a match was found and the provided object was replaced with a cached one.</para>
    /// </summary>
    /// <param name="key">The object to cache-check.</param>
    /// <param name="value">The value linked with the provided key.</param>
    public bool Check(ref K key, ref V value)
    {
        // Check cache for a matching object.
        for (int i = 0; i < _pairs.Length; i++)
        {
            ref Pair pair = ref _pairs[i];

            // Compare the key item only.
            if (pair.Key.Equals(key))
            {
                if(key is IDisposable kDisposable)
                    kDisposable.Dispose();

                if (value is IDisposable vDisposable)
                    vDisposable.Dispose();

                // Replace the provided object with the cached one.
                key = pair.Key;
                value = pair.Value;
                return true;
            }
        }

        // Add the new key and value to the cache.
        if(value != null)
            Add(ref key, ref value);

        return false;
    }

    /// <summary>
    /// Adds the provided <paramref name="key"/> to the cache without checking for duplicates.
    /// </summary>
    /// <param name="key">The key of the object to add to the cache.</param>
    /// <param name="value">The object to add to the cache.</param>
    public void Add(ref readonly K key, ref readonly V value)
    {
        int index;

        if (_free.Count > 0)
        {
            index = _free.Pop();
        }
        else
        {
            if (_count == _pairs.Length)
                Array.Resize(ref _pairs, _pairs.Length * 2);

            index = _count++;
        }

        _pairs[index] = new Pair()
        {
            Key = key,
            Value = value
        };
    }

    /// <summary>
    /// Removes an object from the cache. If a match was not found to remove, the method returns false.
    /// </summary>
    /// <param name="key">The object remove from the cache.</param>
    /// <returns>True if a match was removed from the cache.</returns>
    public bool Remove(ref readonly K key)
    {
        for (int i = 0; i < _pairs.Length; i++)
        {
            ref Pair pair = ref _pairs[i];

            if (pair.Key.Equals(key))
            {
                _pairs[i] = default;
                _free.Push(i);
                return true;
            }
        }

        return false;
    }
}
