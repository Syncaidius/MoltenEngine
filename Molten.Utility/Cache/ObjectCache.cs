using System.Collections;

namespace Molten.Cache;

/// <summary>
/// Provides a <see cref="EngineObject"/> caching system to help prevent duplication.
/// <para>For accurate cache checks, <see cref="IEquatable{T}"/> should be correctly implemented.</para>
/// </summary>
public class ObjectCache
{
    Dictionary<Type, IList> _caches = new Dictionary<Type, IList>();

    /// <summary>
    /// Caches the provided object, or disposes it and replaces it with the matching cached object.
    /// <para>Returns false if a match was not found in the cache. 
    /// Returns true if a match was found and the provided object was replaced with a cached one.</para>
    /// </summary>
    /// <typeparam name="T">The type of object to be cached.</typeparam>
    /// <param name="obj">The object to cache-check.</param>
    public bool Check<T>(ref T obj)
        where T : EngineObject, IEquatable<T>
    {
        // Retrieve correct cache list.
        List<T> cache;
        if (_caches.TryGetValue(typeof(T), out IList cacheObj))
        {
            cache = cacheObj as List<T>;

            // Check cache for a matching object.
            foreach (T item in cache)
            {
                if (obj.Equals(item))
                {
                    obj.Dispose();
                    obj = item;
                    return true;
                }
            }
        }
        else
        {
            cache = new List<T>();
            _caches.Add(typeof(T), cache);
        }

        // No match found. Add the provided object to the cache.
        cache.Add(obj);
        return false;
    }

    /// <summary>
    /// Adds the provided <paramref name="obj"/> to the cache without checking for duplicates.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="EngineObject"/> to be added.</typeparam>
    /// <param name="obj"></param>
    public void Add<T>(ref readonly T obj)
        where T : EngineObject, IEquatable<T>
    {
        // Retrieve correct cache list.
        List<T> cache;
        if (_caches.TryGetValue(typeof(T), out IList cacheObj))
        {
            cache = cacheObj as List<T>;
        }
        else
        {
            cache = new List<T>();
            _caches.Add(typeof(T), cache);
        }

        cache.Add(obj);
    }
}
