using Molten.Comparers;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Molten.Graphics;

/// <summary>An object type which stores objects against <see cref="Type"/> keys.</summary>
public class ShaderLayoutCache : EngineObject
{
    public delegate ShaderIOLayout NewLayoutCallback(uint elementCount);

    public delegate void NewElementCallback(VertexElementAttribute att, ShaderIOLayout structure, uint index, uint byteOffset);

    private class FieldElement
    {
        public FieldInfo Info;
        public IntPtr Offset;
    }

    static IntPtrComparer _ptrComparer = new IntPtrComparer();

    NewLayoutCallback _newCallback;
    NewElementCallback _newElementCallback;
    ConcurrentDictionary<Type, ShaderIOLayout> _cacheByVertexType;
    ConcurrentDictionary<ulong, ShaderIOLayout> _cache;

    /// <summary>
    /// Creates a new instance of <see cref="ObjectCache{K, V}"/>
    /// </summary>
    public ShaderLayoutCache(NewLayoutCallback newCallback, NewElementCallback newElementCallback)
    {
        _cacheByVertexType = new ConcurrentDictionary<Type, ShaderIOLayout>();
        _cache = new ConcurrentDictionary<ulong, ShaderIOLayout>();
        _newCallback = newCallback;
        _newElementCallback = newElementCallback;
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

    /// <summary>Gets a <see cref="ShaderIOLayout"/> from the cache using the specified <typeparamref name="T"/> vertex type. 
    /// If an existing one is not found, a new one will be created matching the structure of <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The type to use as a key.</typeparam>
    /// <returns></returns>
    public ShaderIOLayout GetVertexLayout<T>()
    {
        Type vertexType = typeof(T);
        if (!typeof(IVertexType).IsAssignableFrom(vertexType))
            throw new Exception($"The specified vertex type must implement or derive {vertexType.Name}.");

        if (!_cacheByVertexType.TryGetValue(vertexType, out ShaderIOLayout value))
        {
            value = CreateVertexLayout(vertexType);
            _cacheByVertexType.TryAdd(vertexType, value);
            _cache.TryAdd(value.EOID, value);
        }

        return value;
    }

    private ShaderIOLayout CreateVertexLayout(Type t)
    {
        List<FieldElement> fieldElements = new List<FieldElement>();

        // Retrieve the offset of each field member in the struct, which we can then use for sorting them.
        FieldInfo[] fields = t.GetFields();
        foreach (FieldInfo info in fields)
        {
            FieldElement el = new FieldElement()
            {
                Info = info,
                Offset = Marshal.OffsetOf(t, info.Name),
            };
            fieldElements.Add(el);
        }

        fieldElements = fieldElements.OrderBy(e => e.Offset, _ptrComparer).ToList();

        // Count out how many elements we have.
        uint eCount = 0;
        for (int ec = 0; ec < fieldElements.Count; ec++)
            eCount += fieldElements[ec] != null ? 1U : 0;

        ShaderIOLayout structure = _newCallback(eCount);
        if (structure == null)
            throw new Exception("The current VertexFormatCache callback returned a null ShaderIOLayout.");

        uint sizeOf = 0;
        eCount = 0;

        // Now figure out what kind of InputElement each field represents.
        foreach (FieldElement e in fieldElements)
        {
            VertexElementAttribute att = e.Info.GetCustomAttribute<VertexElementAttribute>();
            if (att == null)
            {
                // TODO Attempt to figure out what the element is or throw an exception if we're unable to.
            }
            else
            {
                structure.Metadata[eCount].Name = GetSemanticName(att);
                structure.Metadata[eCount].SemanticIndex = att.SemanticIndex;
                _newElementCallback(att, structure, eCount, sizeOf);

                sizeOf += att.Type.ToGraphicsFormat().BytesPerPixel();
            }

            eCount++;
        }

        return structure;
    }

    private string GetSemanticName(VertexElementAttribute att)
    {
        switch (att.Usage)
        {
            case VertexElementUsage.PixelPosition:
                return "SV_POSITION";

            case VertexElementUsage.Position:
                return "POSITION";

            case VertexElementUsage.Color:
                return "COLOR";

            case VertexElementUsage.Normal:
                return "NORMAL";

            case VertexElementUsage.TextureCoordinate:
                return "TEXCOORD";

            case VertexElementUsage.BlendIndices:
                return "BLENDINDICES";

            case VertexElementUsage.BlendWeight:
                return "BLENDWEIGHT";

            case VertexElementUsage.Binormal:
                return "BINORMAL";

            case VertexElementUsage.Tangent:
                return "TANGENT";

            case VertexElementUsage.PointSize:
                return "PSIZE";

            case VertexElementUsage.VertexID:
                return "SV_VERTEXID";

            case VertexElementUsage.InstanceID:
                return "SV_INSTANCEID";

            case VertexElementUsage.Custom:
                return att.CustomSemantic;

            default:
                throw new NotSupportedException($"Unsupported vertex element usage: {att.Usage}");
        }
    }
}
