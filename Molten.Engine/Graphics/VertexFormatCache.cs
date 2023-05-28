using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using Molten.Comparers;

namespace Molten.Graphics
{
    /// <summary>An object type which stores objects against <see cref="Type"/> keys.</summary>
    /// <typeparam name="IO">The <see cref="Type"/> of <see cref="ShaderIOLayout"/> to use when initializing new <see cref="VertexFormat"/> instances..</typeparam>
    public class VertexFormatCache<IO> : EngineObject
        where  IO : ShaderIOLayout
    {
        public delegate ShaderIOLayout VertexFormatNewStructureCallback(uint elementCount);

        public delegate void VertexFormatNewElementCallback(VertexElementAttribute att, ShaderIOLayout structure, uint index, uint byteOffset);

        private class FieldElement
        {
            public FieldInfo Info;
            public IntPtr Offset;
        }

        ConcurrentDictionary<Type, VertexFormat> Cache { get; }

        ConcurrentDictionary<ulong, VertexFormat> CacheByID { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> of keys accepted. Valid keys are assignable to this type.
        /// </summary>
        public Type KeyType { get; }

        static IntPtrComparer _ptrComparer = new IntPtrComparer();

        VertexFormatNewStructureCallback _newCallback;
        VertexFormatNewElementCallback _newElementCallback;

        /// <summary>
        /// Creates a new instance of <see cref="ObjectCache{K, V}"/>
        /// </summary>
        public VertexFormatCache(VertexFormatNewStructureCallback newCallback, VertexFormatNewElementCallback newElementCallback)
        {
            KeyType = typeof(IVertexType);
            Cache = new ConcurrentDictionary<Type, VertexFormat>();
            CacheByID = new ConcurrentDictionary<ulong, VertexFormat>();
            _newCallback = newCallback;
            _newElementCallback = newElementCallback;
        }

        protected override void OnDispose()
        {
            foreach (KeyValuePair<Type, VertexFormat> kv in Cache)
                kv.Value.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eoid">The engine-object-identifier (EOID) of the to be retrieved.</param>
        /// <returns></returns>
        public VertexFormat GetByID(ulong eoid)
        {
            if (CacheByID.TryGetValue(eoid, out VertexFormat format))
                return format;
            else
                return null;
        }

        /// <summary>Gets an object from the cache using the specified <typeparamref name="IVertexType"/> key.</summary>
        /// <typeparam name="T">The type to use as a key.</typeparam>
        /// <returns></returns>
        public VertexFormat Get<T>()
        {
            Type kt = typeof(T);
            return Get(kt);
        }

        public VertexFormat Get(Type getKeyType)
        {
            if (!KeyType.IsAssignableFrom(getKeyType))
                throw new Exception($"The specified vertex type must implement or derive {KeyType.Name}.");

            if (!Cache.TryGetValue(getKeyType, out VertexFormat value))
            {
                value = GetNewFormat(getKeyType);
                Cache.TryAdd(getKeyType, value);
                CacheByID.TryAdd(value.EOID, value);
            }

            return value;
        }

        private VertexFormat GetNewFormat(Type t)
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

            return new VertexFormat(structure, sizeOf);
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
}
