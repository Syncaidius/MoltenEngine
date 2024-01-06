using System.Collections.Concurrent;

namespace Molten;

/// <summary>
/// 
/// </summary>
/// <typeparam name="K">The <see cref="Type"/> of cache keys.</typeparam>
/// <typeparam name="V">The type of <see cref="EngineObject"/> to be cached.</typeparam>
public class ObjectCache<K, V> 
    where V : EngineObject
{
    ConcurrentDictionary<K, V> Cache { get; }
    ConcurrentDictionary<ulong, V> CacheByID { get; }

    Func<V> _createCallback;

    /// <summary>
    /// Creates a new instance of <see cref="ObjectCache{K, V}"/>
    /// </summary>
    /// <param name="createCallback"></param>
    public ObjectCache(Func<V> createCallback)
    {
        _createCallback = createCallback;
        Cache = new ConcurrentDictionary<K, V>();
        CacheByID = new ConcurrentDictionary<ulong, V>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eoid">The engine-object-identifier (EOID) of the <typeparamref name="V"/> to be retrieved.</param>
    /// <returns></returns>
    internal V GetByID(ulong eoid)
    {
        if (CacheByID.TryGetValue(eoid, out V format))
            return format;
        else
            return null;
    }

    /// <summary>Gets an object from the cache using the specified <typeparamref name="K"/> key.</summary>
    /// <param name="key">The <typeparamref name="K"/> key of the value to be retrieved.</param>
    /// <returns></returns>
    internal V Get(K key)
    {
        if (!Cache.TryGetValue(key, out V value))
        {
            value = _createCallback();
            Cache.TryAdd(key, value);
            CacheByID.TryAdd(value.EOID, value);
        }

        return default(V);
    }
}
