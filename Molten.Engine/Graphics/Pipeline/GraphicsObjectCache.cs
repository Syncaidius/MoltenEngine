using System.Collections;

namespace Molten.Graphics;

public class GraphicsObjectCache
{
    Dictionary<Type, IList> _caches = new Dictionary<Type, IList>();

    /// <summary>
    /// Caches the provided object, or replaces with a matching duplicate.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    public void Object<T>(ref T obj)
        where T : GraphicsObject, IEquatable<T>
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
                    return;
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
    }

    /// <summary>
    /// Adds the provided <paramref name="obj"/> to the cache without checking for duplicates.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="GraphicsObject"/> to be added.</typeparam>
    /// <param name="obj"></param>
    public void Add<T>(T obj)
        where T : GraphicsObject, IEquatable<T>
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

        // No match found. Add the provided object to the cache.
        cache.Add(obj);
    }
}
