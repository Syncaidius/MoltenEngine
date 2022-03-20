using Molten.Collections;

namespace Molten
{
    /// <summary>An object type which stores objects against <see cref="Type"/> keys. Disposing of the cache will also dispose of all held <typeparamref name="K"/> objects.</summary>
    /// <typeparam name="K">The <see cref="Type"/> of cache keys.</typeparam>
    /// <typeparam name="V">The type of <see cref="EngineObject"/> to be cached.</typeparam>
    public class TypedObjectCache<K, V> : EngineObject
        where V : EngineObject
    {
        ThreadedDictionary<Type, V> Cache { get; }
        ThreadedDictionary<ulong, V> CacheByID { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> of keys accepted. Valid keys are assignable to this type.
        /// </summary>
        public Type KeyType { get; }

        Func<Type, V> _createCallback;

        /// <summary>
        /// Creates a new instance of <see cref="ObjectCache{K, V}"/>
        /// </summary>
        /// <param name="createCallback"></param>
        public TypedObjectCache(Func<Type, V> createCallback)
        {
            KeyType = typeof(K);
            _createCallback = createCallback;
            Cache = new ThreadedDictionary<Type, V>();
            CacheByID = new ThreadedDictionary<ulong, V>();
        }

        protected override void OnDispose()
        {
            foreach (KeyValuePair<Type, V> kv in Cache)
                kv.Value.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eoid">The engine-object-identifier (EOID) of the <typeparamref name="V"/> to be retrieved.</param>
        /// <returns></returns>
        public V GetByID(ulong eoid)
        {
            if (CacheByID.TryGetValue(eoid, out V format))
                return format;
            else
                return null;
        }

        /// <summary>Gets an object from the cache using the specified <typeparamref name="K"/> key.</summary>
        /// <typeparam name="T">The type to use as a key.</typeparam>
        /// <returns></returns>
        public V Get<T>()
        {
            Type kt = typeof(T);
            return Get(kt);
        }

        public V Get(Type getKeyType)
        {
            if (!KeyType.IsAssignableFrom(getKeyType))
                throw new Exception($"The specified vertex type must implement or derive {KeyType.Name}.");

            if (!Cache.TryGetValue(getKeyType, out V value))
            {
                value = _createCallback(getKeyType);
                Cache.Add(getKeyType, value);
                CacheByID.Add(value.EOID, value);
            }

            return value;
        }
    }
}
