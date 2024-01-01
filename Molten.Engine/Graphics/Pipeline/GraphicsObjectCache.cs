using System.Collections;

namespace Molten.Graphics
{
    public class GraphicsObjectCache
    {
        Dictionary<Type, IList> _caches = new Dictionary<Type, IList>();

        /// <summary>
        /// Caches the provided object, or replaces with a matching duplicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="DESC"></typeparam>
        /// <param name="obj"></param>
        public void Object<T, DESC>(ref T obj)
            where T : GraphicsObject, IEquatable<T>, IEquatable<DESC>
            where DESC : struct
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
        /// <typeparam name="DESC">The description or info value type that represents the provided object.</typeparam>
        /// <param name="obj"></param>
        public void Add<T, DESC>(T obj)
            where T : GraphicsObject, IEquatable<T>, IEquatable<DESC>
            where DESC : struct
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

        /// <summary>
        /// Retrieves a matching <typeparamref name="T"/> instance from the cache. If no match is found, null is returned instead.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="GraphicsObject"/> to match.</typeparam>
        /// <typeparam name="DESC">The type of description or info value to be matched.</typeparam>
        /// <param name="desc">The description or info value to match.</param>
        /// <returns></returns>
        public T Get<T, DESC>(ref DESC desc)
            where T : GraphicsObject, IEquatable<T>, IEquatable<DESC>
            where DESC : struct
        {
            // Retrieve correct cache list.
            List<T> cache;
            if (_caches.TryGetValue(typeof(T), out IList cacheObj))
            {
                cache = cacheObj as List<T>;                
                
                // Check cache for a matching object.
                foreach (T obj in cache)
                {
                    if (obj.Equals(desc))
                        return obj;
                }
            }

            return null;
        }
    }
}
