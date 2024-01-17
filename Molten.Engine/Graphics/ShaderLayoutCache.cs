using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Molten.Graphics;

/// <summary>An object type which stores objects against <see cref="Type"/> keys.</summary>
public abstract class ShaderLayoutCache : EngineObject
{
    ConcurrentDictionary<Type, ShaderIOLayout> _cacheByVertexType;
    ConcurrentDictionary<ulong, ShaderIOLayout> _cache;

    /// <summary>
    /// Creates a new instance of <see cref="ObjectCache{K, V}"/>
    /// </summary>
    public ShaderLayoutCache()
    {
        _cacheByVertexType = new ConcurrentDictionary<Type, ShaderIOLayout>();
        _cache = new ConcurrentDictionary<ulong, ShaderIOLayout>();
    }

    protected override void OnDispose()
    {
        foreach (KeyValuePair<Type, ShaderIOLayout> kv in _cacheByVertexType)
            kv.Value.Dispose();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eoid">The engine-object-identifier (EOID) of the to be retrieved.</param>
    /// <returns></returns>
    public ShaderIOLayout GetByID(ulong eoid)
    {
        if (_cache.TryGetValue(eoid, out ShaderIOLayout format))
            return format;
        else
            return null;
    }

    /// <summary>
    /// Checks if a matching <see cref="ShaderIOLayout"/> exists in the cache. If true, the provided one is disposed and replaced with the cached one.
    /// </summary>
    /// <param name="layout">The layout to be cached.</param>
    /// <returns></returns>
    public void Cache(ref ShaderIOLayout layout)
    {
        if(layout == null)
            throw new ArgumentNullException(nameof(layout));

        List<ShaderIOLayout> cached = _cacheByVertexType.Values.ToList();
        for(int i = 0; i < cached.Count; i++)
        {
            if (cached[i].Equals(layout))
            {
                layout.Dispose();
                layout = cached[i];
                return;
            }
        }
    }

    /// <summary>Gets a <typeparamref name="T"/> from the cache using the specified <typeparamref name="V"/> vertex type. 
    /// If an existing one is not found, a new one will be created matching the structure of <typeparamref name="V"/>.</summary>
    /// <typeparam name="V">The type to use as a key.</typeparam>
    /// <returns></returns>
    public  ShaderIOLayout GetVertexLayout<V>()
        where V : struct, IVertexType
    {
        Type vertexType = typeof(V);
        if (!_cacheByVertexType.TryGetValue(vertexType, out ShaderIOLayout value))
        {
            value = Create();
            value.Build<V>();

            if (_cacheByVertexType.TryAdd(vertexType, value))
                _cache.TryAdd(value.EOID, value);
        }

        return value;
    }

    /// <summary>
    /// Creates a new <see cref="ShaderIOLayout"/> instance.
    /// </summary>
    /// <returns></returns>
    public abstract ShaderIOLayout Create();
}

public class ShaderLayoutCache<T> : ShaderLayoutCache
    where T : ShaderIOLayout, new()
{
    public override ShaderIOLayout Create() => new T();
}
