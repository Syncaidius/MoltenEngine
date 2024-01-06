namespace Molten.Collections;

/// <summary>A thread-safe collection for handling object pooling.</summary>
/// <typeparam name="T"></typeparam>
public class ObjectPool<T> : IDisposable where T : IPoolable
{
    ThreadedQueue<T> _pool;
    Func<T> _generator;

    /// <summary>Creates a new instance of ObjectPool.</summary>
    /// <param name="generator">The method that will be used to create a new instance of the type the pool 
    /// stores, if one is needed.</param>
    public ObjectPool(Func<T> generator)
    {
        _pool = new ThreadedQueue<T>();

        if (generator == null)
            throw new ArgumentNullException("Generator cannot be null.");
        else
            _generator = generator;
    }

    /// <summary>Returns a recycled instance or generates a new one if none are available.</summary>
    /// <returns></returns>
    public T GetInstance()
    {
        T instance = default;
        if (_pool.TryDequeue(out instance) == false)
            instance = _generator();

        return instance;
    }

    /// <summary>Recycles an instance back into the pool for later re-use.</summary>
    /// <param name="item">The item to recycle.</param>
    public void Recycle(T item)
    {
        item.ClearForPool();
        _pool.Enqueue(item);
    }

    public void Dispose()
    {
        //disposable of all disposable objects.
        while (_pool.Count > 0)
        {
            T o = default;
            if (_pool.TryDequeue(out o))
            {
                IDisposable d = (IDisposable)o;
                if (d != null)
                    d.Dispose();
            }
        }
    }

}

/// <summary>An interface which defines a poolable item for use with ObjectPool.</summary>
public interface IPoolable
{
    /// <summary>
    /// Clears the object's state. This is automatically called by an <see cref="ObjectPool{T}"/>
    /// </summary>
    void ClearForPool();
}
