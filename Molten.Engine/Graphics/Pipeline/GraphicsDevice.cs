using System.Runtime.CompilerServices;
using Molten.Collections;

namespace Molten.Graphics
{
    /// <summary>
    /// The base class for an API-specific implementation of a graphics device, which provides command/resource access to a GPU.
    /// </summary>
    public abstract class GraphicsDevice : EngineObject
    {
        long _allocatedVRAM;
        ThreadedQueue<GraphicsObject> _objectsToDispose;
        Dictionary<Type, Dictionary<StructKey, GraphicsObject>> _objectCache;

        /// <summary>
        /// Creates a new instance of <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <param name="settings">The <see cref="GraphicsSettings"/> to bind to the device.</param>
        /// <param name="log">The <see cref="Logger"/> to use for outputting information.</param>
        protected GraphicsDevice(GraphicsSettings settings, Logger log)
        {
            Settings = settings;
            Log = log;
            _objectsToDispose = new ThreadedQueue<GraphicsObject>();
            _objectCache = new Dictionary<Type, Dictionary<StructKey, GraphicsObject>>();
        }

        internal void Initialize()
        {
            OnInitialize();

            ShaderSamplerParameters samplerParams = new ShaderSamplerParameters(SamplerPreset.Default);
            DefaultSampler = CreateSampler(ref samplerParams);
        }

        protected abstract void OnInitialize();

        internal void DisposeMarkedObjects()
        {
            while (_objectsToDispose.TryDequeue(out GraphicsObject obj))
                obj.GraphicsRelease();
        }

        public void MarkForRelease(GraphicsObject pObject)
        {
            if (IsDisposed)
                pObject.GraphicsRelease();
            else
                _objectsToDispose.Enqueue(pObject);
        }

        protected override void OnDispose()
        {
            DisposeMarkedObjects();
            Cmd?.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objKey"></param>
        /// <param name="newObj"></param>
        public T CacheObject<T>(StructKey objKey, T newObj)
            where T : GraphicsObject
        {
            if (!_objectCache.TryGetValue(typeof(T), out Dictionary<StructKey, GraphicsObject> objects))
            {
                objects = new Dictionary<StructKey, GraphicsObject>();
                _objectCache.Add(typeof(T), objects);
            }

            if (newObj != null)
            {
                foreach (StructKey key in objects.Keys)
                {
                    if (key.Equals(objKey))
                    {
                        // Dispose of the new object, we found an existing match.
                        newObj.Dispose();
                        return objects[key] as T;
                    }
                }

                // If we reach here, object has no match in the cache. Add it
                objects.Add(objKey.Clone(), newObj);
            }

            return newObj;
        }

        /// <summary>
        /// Requests a new <see cref="HlslPass"/> from the current <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <returns></returns>
        public abstract HlslPass CreateShaderPass(HlslShader shader, string name = null);

        /// <summary>
        /// Requests a new <see cref="ShaderSampler"/> from the current <see cref="GraphicsDevice"/>, with the implementation's default sampler settings.
        /// </summary>
        /// <param name="parameters">The parameters to use when creating the new <see cref="ShaderSampler"/>.</param>
        /// <returns></returns>
        public ShaderSampler CreateSampler(ref ShaderSamplerParameters parameters)
        {
            StructKey<ShaderSamplerParameters> key = new StructKey<ShaderSamplerParameters>(ref parameters);
            ShaderSampler newSampler = OnCreateSampler(ref parameters);
            ShaderSampler result = CacheObject(key, newSampler);

            if (result != newSampler)
            {
                newSampler.Dispose();
                key.Dispose();
            }

            return result;
        }

        protected abstract ShaderSampler OnCreateSampler(ref ShaderSamplerParameters parameters);

        public IVertexBuffer CreateVertexBuffer<T>(T[] data, BufferMode mode = BufferMode.Immutable)
            where T : unmanaged, IVertexType
        {
            return CreateVertexBuffer(mode, (uint)data.Length, data);
        }

        public abstract IVertexBuffer CreateVertexBuffer<T>(BufferMode mode, uint numVertices, T[] initialData = null)
            where T : unmanaged, IVertexType;

        public IIndexBuffer CreateIndexBuffer(ushort[] data, BufferMode mode = BufferMode.Immutable)
        {
            return CreateIndexBuffer(mode, (uint)data.Length, data);
        }

        public IIndexBuffer CreateIndexBuffer(uint[] data, BufferMode mode = BufferMode.Immutable)
        {
            return CreateIndexBuffer(mode, (uint)data.Length, data);
        }

        public abstract IIndexBuffer CreateIndexBuffer(BufferMode mode, uint numIndices, ushort[] initialData);

        public abstract IIndexBuffer CreateIndexBuffer(BufferMode mode, uint numIndices, uint[] initialData = null);

        public IStructuredBuffer CreateStructuredBuffer<T>(T[] data, BufferMode mode = BufferMode.Immutable)
            where T : unmanaged
        {
            return CreateStructuredBuffer(mode, (uint)data.Length, false, true, data);
        }

        public abstract IStructuredBuffer CreateStructuredBuffer<T>(BufferMode mode, uint numElements, bool allowUnorderedAccess, bool isShaderResource, T[] initialData = null)
            where T : unmanaged;

        public abstract IStagingBuffer CreateStagingBuffer(StagingBufferFlags staging, uint byteCapacity);

        /// <summary>Track a VRAM allocation.</summary>
        /// <param name="bytes">The number of bytes that were allocated.</param>
        public void AllocateVRAM(long bytes)
        {
            Interlocked.Add(ref _allocatedVRAM, bytes);
        }

        /// <summary>Track a VRAM deallocation.</summary>
        /// <param name="bytes">The number of bytes that were deallocated.</param>
        public void DeallocateVRAM(long bytes)
        {
            Interlocked.Add(ref _allocatedVRAM, -bytes);
        }

        /// <summary>
        /// Gets the amount of VRAM that has been allocated on the current <see cref="GraphicsDevice"/>. 
        /// <para>For a software or integration device, this may be system memory (RAM).</para>
        /// </summary>
        internal long AllocatedVRAM => _allocatedVRAM;

        /// <summary>
        /// Gets the <see cref="Logger"/> that is bound to the current <see cref="GraphicsDevice"/> for outputting information.
        /// </summary>
        public Logger Log { get; }

        /// <summary>
        /// Gets the <see cref="GraphicsSettings"/> bound to the current <see cref="GraphicsDevice"/>.
        /// </summary>
        public GraphicsSettings Settings { get; }

        /// <summary>
        /// Gets the <see cref="IDisplayAdapter"/> that the current <see cref="GraphicsDevice"/> is bound to.
        /// </summary>
        public abstract IDisplayAdapter Adapter { get; }

        /// <summary>
        /// Gets the <see cref="GraphicsDisplayManager"/> that owns the current <see cref="GraphicsDevice"/>.
        /// </summary>
        public abstract GraphicsDisplayManager DisplayManager { get; }

        /// <summary>
        /// The main <see cref="GraphicsCommandQueue"/> of the current <see cref="GraphicsDevice"/>. This is used for issuing immediate commands to the GPU.
        /// </summary>
        public abstract GraphicsCommandQueue Cmd { get; }

        public ShaderSampler DefaultSampler { get; private set; }
    }

    /// <summary>
    /// A more advanced version of <see cref="GraphicsDevice"/> which manages the allocation and releasing of an unsafe object pointer, exposed via <see cref="Ptr"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe abstract class GraphicsDevice<T> : GraphicsDevice
        where T : unmanaged
    {
        T* _ptr;

        protected GraphicsDevice(GraphicsSettings settings, Logger log, bool allocate) :
            base(settings, log)
        {
            if (allocate)
                _ptr = EngineUtil.Alloc<T>();
        }

        protected override void OnDispose()
        {
            EngineUtil.Free(ref _ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(GraphicsDevice<T> device)
        {
            return *device.Ptr;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T*(GraphicsDevice<T> device)
        {
            return device._ptr;
        }

        /// <summary>
        /// The underlying, native device pointer.
        /// </summary>
        public T* Ptr => _ptr;

        /// <summary>
        /// Gets a protected reference to the underlying device pointer.
        /// </summary>
        protected ref T* PtrRef => ref _ptr;
    }
}
