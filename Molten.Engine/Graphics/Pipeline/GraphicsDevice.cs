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


        GraphicsState _defaultState;
        GraphicsSampler _defaultSampler;

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

            GraphicsStateParameters defaultParams = new GraphicsStateParameters(GraphicsStatePreset.Default);
            _defaultState = CreateState(ref defaultParams);

            GraphicsSamplerParameters samplerParams = new GraphicsSamplerParameters(SamplerPreset.Default);
            _defaultSampler = CreateSampler(ref samplerParams);
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
        /// <param name="obj"></param>
        public T CacheObject<T>(StructKey objKey, T obj)
            where T : GraphicsObject
        {
            if (!_objectCache.TryGetValue(typeof(T), out Dictionary<StructKey, GraphicsObject> objects))
            {
                objects = new Dictionary<StructKey, GraphicsObject>();
                _objectCache.Add(typeof(T), objects);
            }

            if (obj != null)
            {
                foreach (StructKey key in objects.Keys)
                {
                    if (key.Equals(objKey))
                        return objects[key] as T;
                }

                // If we reach here, object has no match in the cache. Add it
                objects.Add(objKey, obj);
            }

            return obj;
        }

        /// <summary>
        /// Requests a new <see cref="GraphicsState"/> from the current <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <returns></returns>
        public GraphicsState CreateState(ref GraphicsStateParameters parameters)
        {
            StructKey<GraphicsStateParameters> key = new StructKey<GraphicsStateParameters>(ref parameters);
            GraphicsState newState = OnCreateState(ref parameters);
            GraphicsState result = CacheObject(key, newState);

            if (result != newState)
            {
                newState.Dispose();
                key.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Requests a new <see cref="GraphicsSampler"/> from the current <see cref="GraphicsDevice"/>, with the implementation's default sampler settings.
        /// </summary>
        /// <param name="parameters">The parameters to use when creating the new <see cref="GraphicsSampler"/>.</param>
        /// <returns></returns>
        public GraphicsSampler CreateSampler(ref GraphicsSamplerParameters parameters)
        {
            StructKey<GraphicsSamplerParameters> key = new StructKey<GraphicsSamplerParameters>(ref parameters);
            GraphicsSampler newSampler = OnCreateSampler(ref parameters);
            GraphicsSampler result = CacheObject(key, newSampler);

            if (result != newSampler)
            {
                newSampler.Dispose();
                key.Dispose();
            }

            return result;
        }

        protected abstract GraphicsState OnCreateState(ref GraphicsStateParameters parameters);

        protected abstract GraphicsSampler OnCreateSampler(ref GraphicsSamplerParameters parameters);

        public abstract ShaderComposition CreateShaderComposition(ShaderType type, HlslShader parent);

        public abstract IGraphicsBuffer CreateBuffer(GraphicsBufferFlags flags, BufferMode mode, uint byteCapacity, uint stride = 0);

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

        public GraphicsState DefaultState { get; private set; }

        public GraphicsSampler DefaultSampler { get; private set; }
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
