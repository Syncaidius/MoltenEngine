using System.Reflection;
using System.Runtime.CompilerServices;
using Molten.Collections;
using Molten.IO;

namespace Molten.Graphics
{
    /// <summary>
    /// The base class for an API-specific implementation of a graphics device, which provides command/resource access to a GPU.
    /// </summary>
    public abstract partial class GraphicsDevice : EngineObject
    {
        /// <summary>Occurs when a connected <see cref="IDisplayOutput"/> is activated on the current <see cref="GraphicsDevice"/>.</summary>
        public event DisplayOutputChanged OnOutputActivated;

        /// <summary>Occurs when a connected <see cref="IDisplayOutput"/> is deactivated on the current <see cref="GraphicsDevice"/>.</summary>
        public event DisplayOutputChanged OnOutputDeactivated;

        long _allocatedVRAM;
        ThreadedList<GraphicsObject> _disposals;

        Dictionary<Type, Dictionary<StructKey, GraphicsObject>> _objectCache;

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
            _disposals = new ThreadedList<GraphicsObject>();
            _objectCache = new Dictionary<Type, Dictionary<StructKey, GraphicsObject>>();
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

        internal void DisposeMarkedObjects()
        {
            // We want to wait at least a quarter of the target FPS before deleting staging buffers.
            Timing timing = Renderer.Thread.Timing;
            uint framesToWait = (uint)timing.TargetUPS / 4U;

            _disposals.For(_disposals.Count - 1, -1, 0, (index, obj) =>
            {
                ulong age = timing.FrameID - obj.LastUsedFrameID;
                if (age >= framesToWait)
                {
                    obj.GraphicsRelease();
                    _disposals.RemoveAt(index);
                }
            });
        }

        public void MarkForRelease(GraphicsObject obj)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("GraphicsDevice has already been disposed, so it cannot mark GraphicsObject instances for release.");

            _disposals.Add(obj);
        }

        protected override void OnDispose()
        {
            DisposeMarkedObjects();
            Queue?.Dispose();
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

        internal HlslPass CreateShaderPass(HlslShader shader, string name = null)
        {
            return OnCreateShaderPass(shader, name);
        }

        protected abstract HlslPass OnCreateShaderPass(HlslShader shader, string name);

        public GraphicsBuffer CreateVertexBuffer<T>(T[] data, GraphicsResourceFlags flags = GraphicsResourceFlags.None)
            where T : unmanaged, IVertexType
        {
            return CreateVertexBuffer(flags, (uint)data.Length, data);
        }

        public abstract GraphicsBuffer CreateVertexBuffer<T>(GraphicsResourceFlags flags, uint numVertices, T[] initialData = null)
            where T : unmanaged, IVertexType;

        public GraphicsBuffer CreateIndexBuffer(ushort[] data, GraphicsResourceFlags flags = GraphicsResourceFlags.None)
        {
            return CreateIndexBuffer(flags, (uint)data.Length, data);
        }

        public GraphicsBuffer CreateIndexBuffer(uint[] data, GraphicsResourceFlags flags = GraphicsResourceFlags.None)
        {
            return CreateIndexBuffer(flags, (uint)data.Length, data);
        }

        public abstract GraphicsBuffer CreateIndexBuffer(GraphicsResourceFlags flags, uint numIndices, ushort[] initialData);

        public abstract GraphicsBuffer CreateIndexBuffer(GraphicsResourceFlags flags, uint numIndices, uint[] initialData = null);

        public GraphicsBuffer CreateStructuredBuffer<T>(T[] data, GraphicsResourceFlags flags = GraphicsResourceFlags.None)
            where T : unmanaged
        {
            return CreateStructuredBuffer(flags, (uint)data.Length, data);
        }

        public abstract GraphicsBuffer CreateStructuredBuffer<T>(GraphicsResourceFlags flags, uint numElements, T[] initialData = null)
            where T : unmanaged;

        public abstract GraphicsBuffer CreateStagingBuffer(bool allowCpuRead, bool allowCpuWrite, uint byteCapacity);

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
            assembly = assembly ?? typeof(RenderService).Assembly;
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
    }
}
