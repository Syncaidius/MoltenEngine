using System.Reflection;
using Molten.Collections;
using Molten.IO;

namespace Molten.Graphics
{
    /// <summary>
    /// The base class for an API-specific implementation of a graphics device, which provides command/resource access to a GPU.
    /// </summary>
    public abstract partial class GraphicsDevice : EngineObject
    {
        public delegate void FrameBufferSizeChangedHandler(uint oldSize, uint newSize);

        /// <summary>Occurs when a connected <see cref="IDisplayOutput"/> is activated on the current <see cref="GraphicsDevice"/>.</summary>
        public event DisplayOutputChanged OnOutputActivated;

        /// <summary>Occurs when a connected <see cref="IDisplayOutput"/> is deactivated on the current <see cref="GraphicsDevice"/>.</summary>
        public event DisplayOutputChanged OnOutputDeactivated;

        /// <summary>
        /// Invoked when the frame-buffer size is changed for the current <see cref="GraphicsDevice"/>.
        /// </summary>
        public event FrameBufferSizeChangedHandler OnFrameBufferSizeChanged;

        const int INITIAL_BRANCH_COUNT = 3;

        long _allocatedVRAM;
        GraphicsFrame[] _frames;
        uint _frameIndex;
        uint _newFrameBufferSize;
        uint _maxStagingSize;

        ThreadedList<GraphicsObject> _disposals;
        ThreadedList<ISwapChainSurface> _outputSurfaces;

        /// <summary>
        /// Creates a new instance of <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="RenderService"/> that the new graphics device will be bound to.</param>
        protected GraphicsDevice(RenderService renderer, GraphicsManager manager)
        {
            Settings = renderer.Settings.Graphics;
            Renderer = renderer;
            Manager = manager;
            Log = renderer.Log;

            Cache = new GraphicsObjectCache();
            _outputSurfaces = new ThreadedList<ISwapChainSurface>();
            _disposals = new ThreadedList<GraphicsObject>();

            _maxStagingSize = (uint)ByteMath.FromMegabytes(renderer.Settings.Graphics.FrameStagingSize);

            SettingValue<FrameBufferMode> bufferingMode = renderer.Settings.Graphics.FrameBufferMode;
            BufferingMode_OnChanged(bufferingMode.Value, bufferingMode.Value);
            bufferingMode.OnChanged += BufferingMode_OnChanged;
        }

        /// <summary>
        /// Invoked when the minimum supported frame-buffer size needs to be known.
        /// </summary>
        /// <returns></returns>
        protected abstract uint MinimumFrameBufferSize();

        private void BufferingMode_OnChanged(FrameBufferMode oldValue, FrameBufferMode newValue)
        {
            SettingValue<FrameBufferMode> bufferingMode = Settings.FrameBufferMode;
            _newFrameBufferSize = MinimumFrameBufferSize();

            // Does the buffer mode exceed the minimum?
            switch (bufferingMode.Value)
            {
                case FrameBufferMode.Double:
                    _newFrameBufferSize = Math.Max(_newFrameBufferSize, 2);
                    break;

                case FrameBufferMode.Triple:
                    _newFrameBufferSize = Math.Max(_newFrameBufferSize, 3);
                    break;

                case FrameBufferMode.Quad:
                    _newFrameBufferSize = Math.Max(_newFrameBufferSize, 4);
                    break;
            }
        }

        protected void InvokeOutputActivated(IDisplayOutput output)
        {
            OnOutputActivated?.Invoke(output);
        }

        protected void InvokeOutputDeactivated(IDisplayOutput output)
        {
            OnOutputDeactivated?.Invoke(output);
        }

        /// <summary>
        /// Activates a <see cref="IDisplayOutput"/> on the current <see cref="GraphicsDevice"/>.
        /// </summary>
        /// <param name="output">The output to be activated.</param>
        public abstract void AddActiveOutput(IDisplayOutput output);

        /// <summary>
        /// Deactivates a <see cref="IDisplayOutput"/> from the current <see cref="GraphicsDevice"/>. It will still be listed in <see cref="Outputs"/>, if attached.
        /// </summary>
        /// <param name="output">The output to be deactivated.</param>
        public abstract void RemoveActiveOutput(IDisplayOutput output);

        /// <summary>
        /// Removes all active <see cref="IDisplayOutput"/> from the current <see cref="GraphicsDevice"/>. They will still be listed in <see cref="Outputs"/>, if attached.
        /// </summary>
        public abstract void RemoveAllActiveOutputs();

        internal void DisposeMarkedObjects(uint framesToWait, ulong frameID)
        {
            // Are we disposing before the render thread has started?
            _disposals.ForReverse(1, (index, obj) =>
            {
                ulong age = frameID - obj.ReleaseFrameID;
                if (age >= framesToWait)
                {
                    obj.GraphicsRelease();
                    _disposals.RemoveAt(index);
                }
            });
        }

        internal void MarkForRelease(GraphicsObject obj)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("GraphicsDevice has already been disposed, so it cannot mark GraphicsObject instances for release.");

            obj.ReleaseFrameID = Renderer.Profiler.FrameID;
            _disposals.Add(obj);
        }

        protected override void OnDispose()
        {
            if (_frames != null)
            {
                for (int i = 0; i < _frames.Length; i++)
                    _frames[i].Dispose();
            }

            // Dispose of any registered output services.
            _outputSurfaces.For(0, (index, surface) =>
            {
                surface.Dispose();
                return false;
            });

            DisposeMarkedObjects(0,0);
            Queue?.Dispose();
        }

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

        public bool Initialize()
        {
            if (IsInitialized)
                throw new InvalidOperationException("Cannot initialize a GraphicsDevice that has already been initialized.");

            if (OnInitialize())
            {
                IsInitialized = true;
                CheckFrameBufferSize();
            }

            return IsInitialized;
        }

        protected abstract bool OnInitialize();

        private void CheckFrameBufferSize()
        {
            // Do we need to resize the number of buffered frames?
            if (_newFrameBufferSize != FrameBufferSize)
            {
                // Only trigger event if resizing and not initializing. CurrentFrameBufferSize is 0 when uninitialized.
                if (FrameBufferSize > 0)
                    OnFrameBufferSizeChanged?.Invoke(FrameBufferSize, _newFrameBufferSize);

                FrameBufferSize = _newFrameBufferSize;

                // Ensure we have enough staging buffers
                if (_frames == null || _frames.Length < FrameBufferSize)
                {
                    Array.Resize(ref _frames, (int)FrameBufferSize);
                    uint bufferBytes = _maxStagingSize;
                    for (int i = 0; i < _frames.Length; i++)
                    {
                        _frames[i] ??= new GraphicsFrame(INITIAL_BRANCH_COUNT)
                        {
                            StagingBuffer = CreateStagingBuffer(true, true, bufferBytes),
                        };
                    }
                }
            }
        }

        internal void BeginFrame()
        {
            Queue.Profiler.Begin();

            // TODO check if _maxStagingSize has changed due to settings. May need to resize all existing staging buffers.
            CheckFrameBufferSize();

            // If the oldest frame hasn't finished yet, wait for it before replacing it with a new one.
            // This stops the CPU from getting too far ahead of the GPU.
            _frames[_frameIndex].Reset();

            // Ensure we don't have too many tracked frames.
            // TODO Check how many full runs we've done and wait until we've done at least 2 before disposing of any tracked frames.
            //      Reset run count if buffer size is changed.
            if (_frames.Length > FrameBufferSize)
            {
                for (int i = _frames.Length; i < FrameBufferSize; i++)
                {
                    _frames[i].Dispose();
                    _frames[i] = null;
                }
            }
        }

        internal void Begin()
        {
            Queue.Profiler.Begin();
            OnBeginFrame(_outputSurfaces);
        }

        internal void EndFrame(Timing time)
        {
            OnEndFrame(_outputSurfaces);
            Queue.Profiler.End(time);

            _frames[_frameIndex].FrameID = Renderer.Profiler.FrameID;
            _frameIndex = (_frameIndex + 1U) % FrameBufferSize;
        }

        protected abstract void OnBeginFrame(ThreadedList<ISwapChainSurface> surfaces);

        protected abstract void OnEndFrame(ThreadedList<ISwapChainSurface> surfaces);

        /// <summary>
        /// Requests a new <see cref="ShaderSampler"/> from the current <see cref="GraphicsDevice"/>, with the implementation's default sampler settings.
        /// </summary>
        /// <param name="parameters">The parameters to use when creating the new <see cref="ShaderSampler"/>.</param>
        /// <returns></returns>
        public ShaderSampler CreateSampler(ref ShaderSamplerParameters parameters)
        {
            ShaderSampler sampler = OnCreateSampler(ref parameters);
            Cache.Object<ShaderSampler, ShaderSamplerParameters>(ref sampler);
            return sampler;
        }

        protected abstract ShaderSampler OnCreateSampler(ref ShaderSamplerParameters parameters);

        internal HlslPass CreateShaderPass(HlslShader shader, string name = null)
        {
            return OnCreateShaderPass(shader, name);
        }

        protected abstract HlslPass OnCreateShaderPass(HlslShader shader, string name);

        public GraphicsBuffer CreateVertexBuffer<T>(GraphicsResourceFlags flags, uint vertexCapacity, T[] initialData = null)
            where T : unmanaged, IVertexType
        {
            flags |= GraphicsResourceFlags.NoShaderAccess;
            GraphicsBuffer buffer = CreateBuffer(GraphicsBufferType.Vertex, flags, GraphicsFormat.Unknown, vertexCapacity, initialData);
            buffer.VertexFormat = VertexCache.Get<T>();

            return buffer;
        }

        public GraphicsBuffer CreateIndexBuffer(ushort[] data, GraphicsResourceFlags flags = GraphicsResourceFlags.None)
        {
            return CreateIndexBuffer(flags, (uint)data.Length, data);
        }

        public GraphicsBuffer CreateIndexBuffer(uint[] data, GraphicsResourceFlags flags = GraphicsResourceFlags.None)
        {
            return CreateIndexBuffer(flags, (uint)data.Length, data);
        }

        public GraphicsBuffer CreateIndexBuffer(byte[] data, GraphicsResourceFlags flags = GraphicsResourceFlags.None)
        {
            return CreateIndexBuffer(flags, (uint)data.Length, data);
        }

        public GraphicsBuffer CreateIndexBuffer(GraphicsResourceFlags flags, uint indexCapacity, ushort[] initialData)
        {
            return CreateBuffer(GraphicsBufferType.Index, flags, GraphicsFormat.R16_UInt, indexCapacity, initialData);
        }

        public GraphicsBuffer CreateIndexBuffer(GraphicsResourceFlags flags, uint indexCapacity, uint[] initialData = null)
        {
            flags |= GraphicsResourceFlags.NoShaderAccess;
            return CreateBuffer(GraphicsBufferType.Index, flags, GraphicsFormat.R32_UInt, indexCapacity, initialData);
        }

        public GraphicsBuffer CreateIndexBuffer(GraphicsResourceFlags flags, uint indexCapacity, byte[] initialData = null)
        {
            flags |= GraphicsResourceFlags.NoShaderAccess;
            return CreateBuffer(GraphicsBufferType.Index, flags, GraphicsFormat.R8_UInt, indexCapacity, initialData);
        }

        public GraphicsBuffer CreateStructuredBuffer<T>(T[] data, GraphicsResourceFlags flags = GraphicsResourceFlags.None)
            where T : unmanaged
        {
            return CreateStructuredBuffer(flags, (uint)data.Length, data);
        }

        public GraphicsBuffer CreateStructuredBuffer<T>(GraphicsResourceFlags flags, uint elementCapacity, T[] initialData = null)
            where T : unmanaged
        {
            return CreateBuffer(GraphicsBufferType.Structured, flags, GraphicsFormat.Unknown, elementCapacity, initialData);
        }

        public GraphicsBuffer CreateStagingBuffer(bool allowCpuRead, bool allowCpuWrite, uint byteCapacity)
        {
            GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite | GraphicsResourceFlags.NoShaderAccess;

            if (allowCpuRead)
                flags |= GraphicsResourceFlags.CpuRead;

            if (allowCpuWrite)
                flags |= GraphicsResourceFlags.CpuWrite;

            return CreateBuffer<byte>(GraphicsBufferType.Staging, flags, GraphicsFormat.Unknown, byteCapacity, null);
        }

        protected abstract GraphicsBuffer CreateBuffer<T>(GraphicsBufferType type, GraphicsResourceFlags flags, GraphicsFormat format, 
            uint numElements, T[] initialData) where T : unmanaged;

        /// <summary>
        /// Loads an embedded shader from the target assembly. If an assembly is not provided, the current renderer's assembly is used instead.
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <param name="filename"></param>
        /// <param name="assembly">The assembly that contains the embedded shadr. If an assembly is not provided, the current renderer's assembly is used instead.</param>
        /// <returns></returns>
        public ShaderCompileResult LoadEmbeddedShader(string nameSpace, string filename, Assembly assembly = null)
        {
            string src = "";
            assembly ??= typeof(RenderService).Assembly;
            Stream stream = EmbeddedResource.TryGetStream($"{nameSpace}.{filename}", assembly);
            if (stream != null)
            {
                using (StreamReader reader = new StreamReader(stream))
                    src = reader.ReadToEnd();

                stream.Dispose();
            }
            else
            {
                Log.Error($"Attempt to load embedded shader failed: '{filename}' not found in namespace '{nameSpace}' of assembly '{assembly.FullName}'");
                return new ShaderCompileResult();
            }

            return Renderer.Compiler.Compile(src, filename, ShaderCompileFlags.EmbeddedFile, assembly, nameSpace);
        }

        /// <summary>Compiles a set of shaders from the provided source string.</summary>
        /// <param name="source">The source code to be parsed and compiled.</param>
        /// <param name="filename">The name of the source file. Used as a pouint of reference in debug/error messages only.</param>
        /// <returns></returns>
        public ShaderCompileResult CompileShaders(ref string source, string filename = null)
        {
            ShaderCompileFlags flags = ShaderCompileFlags.EmbeddedFile;

            if (!string.IsNullOrWhiteSpace(filename))
            {
                FileInfo fInfo = new FileInfo(filename);
                DirectoryInfo dir = fInfo.Directory;
                flags = ShaderCompileFlags.None;
            }

            return Renderer.Compiler.Compile(source, filename, flags, null, null);
        }

        /// <summary>
        /// Gets the amount of VRAM that has been allocated on the current <see cref="GraphicsDevice"/>. 
        /// <para>For a software or integration device, this may be system memory (RAM).</para>
        /// </summary>
        internal long AllocatedVRAM => _allocatedVRAM;

        /// <summary>
        /// Gets the current frame on the current <see cref="GraphicsDevice"/>.
        /// </summary>
        public GraphicsFrame Frame => _frames[_frameIndex];

        /// <summary>
        /// Gets the <see cref="Logger"/> that is bound to the current <see cref="GraphicsDevice"/> for outputting information.
        /// </summary>
        public Logger Log { get; }

        /// <summary>
        /// Gets the <see cref="GraphicsSettings"/> bound to the current <see cref="GraphicsDevice"/>.
        /// </summary>
        public GraphicsSettings Settings { get; }

        /// <summary>
        /// Gets the <see cref="GraphicsManager"/> that owns the current <see cref="GraphicsDevice"/>.
        /// </summary>
        public GraphicsManager Manager { get; }

        /// <summary>
        /// The main <see cref="GraphicsQueue"/> of the current <see cref="GraphicsDevice"/>. This is used for issuing immediate commands to the GPU.
        /// </summary>
        public abstract GraphicsQueue Queue { get; }

        /// <summary>
        /// Gets the <see cref="RenderService"/> that created and owns the current <see cref="GraphicsDevice"/> instance.
        /// </summary>
        public RenderService Renderer { get; }

        /// <summary>Gets the machine-local device ID of the current <see cref="GraphicsDevice"/>.</summary>
        public abstract DeviceID ID { get; }

        /// <summary>The hardware vendor.</summary>
        public abstract DeviceVendor Vendor { get; }

        /// <summary>
        /// Gets the <see cref="GraphicsDeviceType"/> of the current <see cref="GraphicsDevice"/>.
        /// </summary>
        public abstract GraphicsDeviceType Type { get; }

        /// <summary>Gets a list of all <see cref="IDisplayOutput"/> devices attached to the current <see cref="GraphicsDevice"/>.</summary>
        public abstract IReadOnlyList<IDisplayOutput> Outputs { get; }

        /// <summary>Gets a list of all active <see cref="IDisplayOutput"/> devices attached to the current <see cref="GraphicsDevice"/>.
        /// <para>Active outputs are added via <see cref="AddActiveOutput(IDisplayOutput)"/>.</para></summary>
        public abstract IReadOnlyList<IDisplayOutput> ActiveOutputs { get; }

        /// <summary>
        /// Gets the capabilities of the current <see cref="GraphicsDevice"/>.
        /// </summary>
        public GraphicsCapabilities Capabilities { get; protected set; }

        /// <summary>
        /// Gets the vertex format cache which stores <see cref="VertexFormat"/> instances to help avoid the need to generate multiple instances of the same formats.
        /// </summary>
        public VertexFormatCache VertexCache { get; protected set; }

        /// <summary>
        /// Gets the current frame-buffer size. The value will be between 1 and <see cref="GraphicsSettings.FrameBufferMode"/>, from <see cref="Settings"/>.
        /// </summary>
        public uint FrameBufferSize { get; private set; }

        /// <summary>
        /// Gets the current frame buffer image index. The value will be between 0 and <see cref="GraphicsSettings.FrameBufferMode"/> - 1, from <see cref="Settings"/>.
        /// </summary>
        public uint FrameBufferIndex => _frameIndex;

        /// <summary>
        /// Gets the maximum size of a frame's staging buffer, in bytes.
        /// </summary>
        public uint MaxStagingBufferSize => _maxStagingSize;

        /// <summary>
        /// Gets whether or not the current <see cref="GraphicsDevice"/> is initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets the <see cref="GraphicsObjectCache"/> that is bound to the current <see cref="GraphicsDevice"/>.
        /// </summary>
        public GraphicsObjectCache Cache { get; }
    }
}
