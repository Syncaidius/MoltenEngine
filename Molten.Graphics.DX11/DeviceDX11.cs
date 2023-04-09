using System.Reflection;
using System.Runtime.CompilerServices;
using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Message = Silk.NET.Direct3D11.Message;

namespace Molten.Graphics
{
    /// <summary>A Direct3D 11 graphics device.</summary>
    /// <seealso cref="GraphicsQueueDX11" />
    public unsafe class DeviceDX11 : DeviceDXGI
    {
        ID3D11Device5* _native;
        DeviceBuilderDX11 _builder;
        GraphicsManagerDXGI _displayManager;

        GraphicsQueueDX11 _queue;
        List<GraphicsQueueDX11> _cmdDeferred;

        ID3D11Debug* _debug;
        ID3D11InfoQueue* _debugInfo;

        /// <summary>The adapter to initially bind the graphics device to. Can be changed later.</summary>
        /// <param name="adapter">The physical display adapter to bind the new device to.</param>
        internal DeviceDX11(RenderService renderer, GraphicsManagerDXGI manager, IDXGIAdapter4* adapter, DeviceBuilderDX11 builder) :
            base(renderer, manager, adapter)
        {
            _builder = builder;
            _displayManager = manager;
            _cmdDeferred = new List<GraphicsQueueDX11>();

            VertexFormatCache = new VertexFormatCache<ShaderIOStructureDX11>(
                (elementCount) => new ShaderIOStructureDX11(elementCount),
                (att, structure, index, byteOffset) =>
                {
                    ShaderIOStructureDX11 dxStruct = structure as ShaderIOStructureDX11;
                    dxStruct.VertexElements[index] = new InputElementDesc()
                    {
                        SemanticName = (byte*)SilkMarshal.StringToPtr(dxStruct.Metadata[index].Name),
                        SemanticIndex = att.SemanticIndex,
                        AlignedByteOffset = byteOffset,
                        InstanceDataStepRate = att.InstanceStepRate,
                        InputSlotClass = att.Classification.ToApi(),
                        Format = att.Type.ToGraphicsFormat().ToApi()
                    };
                });
        }

        internal unsafe void ProcessDebugLayerMessages()
        {
            if(_debug != null)
            {
                ulong count = _debugInfo->GetNumStoredMessages();
                for(ulong i = 0; i < count; i++)
                {
                    nuint msgSize = 0;
                    _debugInfo->GetMessageA(i, null, &msgSize);
                    if (msgSize == 0)
                        continue;

                    void* ptrMsg = EngineUtil.Alloc(msgSize);
                    Message* msg = (Message*)ptrMsg;

                    _debugInfo->GetMessageA(i, msg, &msgSize);

                    string desc = SilkMarshal.PtrToString((nint)msg->PDescription, NativeStringEncoding.LPStr);
                    Log.Error($"[DX11 DEBUG] [Frame {Queue.Profiler.FrameID}] [{msg->Severity}] [{msg->Category}] {desc}");
                }

                _debugInfo->ClearStoredMessages();
            }
        }

        internal void Initialize()
        {
            HResult r = _builder.CreateDevice(this, out PtrRef, out ID3D11DeviceContext4* deviceContext);

            if (r.IsFailure)
            {
                Log.Error($"Failed to initialize {nameof(DeviceDX11)}. Code: {r}");
                return;
            }

            if (Settings.EnableDebugLayer)
            {
                Guid guidDebug = ID3D11Debug.Guid;
                void* ptr = null;
                Ptr->QueryInterface(&guidDebug, &ptr);
                _debug = (ID3D11Debug*)ptr;

                Guid guidDebugInfo = ID3D11InfoQueue.Guid;
                _debug->QueryInterface(&guidDebugInfo, &ptr);
                _debugInfo = (ID3D11InfoQueue*)ptr;
                _debugInfo->PushEmptyStorageFilter();
            }

            _queue = new GraphicsQueueDX11(this, deviceContext);
        }

        /// <summary>Queries the underlying texture's interface.</summary>
        /// <typeparam name="Q">The type of object to request in the query.</typeparam>
        /// <returns></returns>
        internal Q* QueryInterface<Q>(void* ptrObject) where Q : unmanaged
        {
            if (ptrObject != null)
            {
                IUnknown* ptr = (IUnknown*)ptrObject;
                Type t = typeof(Q);
                FieldInfo mInfo = t.GetField("Guid");

                if (mInfo == null)
                    throw new Exception("");

                void* result = null;
                Guid guid = (Guid)mInfo.GetValue(null);
                ptr->QueryInterface(&guid, &result);
                return (Q*)result;
            }

            return null;
        }

        /// <summary>Gets a new deferred <see cref="GraphicsQueueDX11"/>.</summary>
        /// <returns></returns>
        internal GraphicsQueueDX11 GetDeferredContext()
        {
            ID3D11DeviceContext3* dc = null;
            _native->CreateDeferredContext3(0, &dc);

            Guid cxt4Guid = ID3D11DeviceContext4.Guid;
            void* ptr4 = null;
            dc->QueryInterface(&cxt4Guid, &ptr4);

            GraphicsQueueDX11 context = new GraphicsQueueDX11(this, (ID3D11DeviceContext4*)ptr4);
            _cmdDeferred.Add(context);
            return context;
        }

        internal void RemoveDeferredContext(GraphicsQueueDX11 queue)
        {
            if (queue.Device != this)
                throw new GraphicsCommandQueueException(queue, "Command list is owned by another graphics queue.");

            if (!queue.IsDisposed)
                queue.Dispose();

            _cmdDeferred.Remove(queue);
        }

        /// <summary>Disposes of the <see cref="DeviceDX11"/> and any deferred contexts and resources bound to it.</summary>
        protected override void OnDispose()
        {
            _queue.Dispose();

            // TODO dispose of all bound IGraphicsResource
            VertexFormatCache.Dispose();

            if (_debug != null)
            {
                ProcessDebugLayerMessages();
                _debug->ReportLiveDeviceObjects(RldoFlags.Detail);
                SilkUtil.ReleasePtr(ref _debugInfo);
                SilkUtil.ReleasePtr(ref _debug);
            }

            base.OnDispose();
        }

        public override IDepthStencilSurface CreateDepthSurface(uint width, uint height, 
            DepthFormat format = DepthFormat.R24G8_Typeless, 
            GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite, 
            uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, bool allowMipMapGen = false, string name = null)
        {
            MSAAQuality msaa = MSAAQuality.CenterPattern;
            return new DepthSurfaceDX11(this, width, height, flags, format, mipCount, arraySize, aaLevel, msaa, allowMipMapGen, name);
        }

        protected override HlslPass OnCreateShaderPass(HlslShader shader, string name = null)
        {
            return new ShaderPassDX11(shader, name);
        }

        public override INativeSurface CreateFormSurface(string formTitle, string formName, uint mipCount = 1)
        {
            return new RenderFormSurface(this, formTitle, formName, mipCount);
        }

        public override INativeSurface CreateControlSurface(string formTitle, string controlName, uint mipCount = 1)
        {
            return new RenderControlSurface(this, formTitle, controlName, mipCount);
        }

        public override IRenderSurface2D CreateSurface(uint width, uint height, 
            GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm, 
            GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite, 
            uint mipCount = 1, 
            uint arraySize = 1, 
            AntiAliasLevel aaLevel = AntiAliasLevel.None, 
            bool allowMipMapGen = false, string name = null)
        {
            MSAAQuality msaa = MSAAQuality.CenterPattern;
            return new RenderSurface2DDX11(this, width, height, flags, format, mipCount, arraySize, aaLevel, msaa, allowMipMapGen, name);
        }

        public override ITexture CreateTexture1D(Texture1DProperties properties, bool allowMipMapGen = false, string name = null)
        {
            return new Texture1DDX11(this, properties.Width, properties.Flags, properties.Format, properties.MipMapLevels, properties.ArraySize, allowMipMapGen, name);
        }

        public override ITexture CreateTexture1D(TextureData data, bool allowMipMapGen = false, string name = null)
        {
            Texture1DDX11 tex = new Texture1DDX11(this, data.Width, data.Flags, data.Format, data.MipMapLevels, data.ArraySize, allowMipMapGen, name);
            tex.SetData(GraphicsPriority.Apply, data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        public override ITexture2D CreateTexture2D(Texture2DProperties properties, bool allowMipMapGen = false, string name = null)
        {
            return new Texture2DDX11(this,
                properties.Width,
                properties.Height,
                properties.Flags,
                properties.Format,
                properties.MipMapLevels,
                properties.ArraySize,
                properties.MultiSampleLevel,
                properties.SampleQuality,
                false,
                properties.Name);
        }

        public override ITexture2D CreateTexture2D(TextureData data, bool allowMipMapGen = false, string name = null)
        {
            Texture2DDX11 tex = new Texture2DDX11(this,
                data.Width,
                data.Height,
                data.Flags,
                data.Format,
                data.MipMapLevels,
                data.ArraySize,
                data.MultiSampleLevel);

            tex.SetData(GraphicsPriority.Apply, data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        public override ITexture3D CreateTexture3D(Texture3DProperties properties, bool allowMipMapGen = false, string name = null)
        {
            return new Texture3DDX11(this,
                properties.Width,
                properties.Height,
                properties.Depth,
                properties.Flags,
                properties.Format,
                properties.MipMapLevels);
        }

        public override ITexture3D CreateTexture3D(TextureData data, bool allowMipMapGen = false, string name = null)
        {
            throw new NotImplementedException();

            // TODO TextureData needs support for 3D data

            /*Texture3D tex = new Texture3D(_renderer,
                data.Width,
                data.Height,
                data.Depth,
                data.Format.ToApi(),
                data.MipMapLevels,
                data.Flags);

            tex.SetData(data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;*/
        }

        public override ITextureCube CreateTextureCube(Texture2DProperties properties, bool allowMipMapGen = false, string name = null)
        {
            uint cubeCount = Math.Max(properties.ArraySize / 6, 1);
            return new TextureCubeDX11(this, properties.Width, properties.Height, properties.Flags, properties.Format, properties.MipMapLevels, cubeCount);
        }

        public override ITextureCube CreateTextureCube(TextureData data, bool allowMipMapGen = false, string name = null)
        {
            uint cubeCount = Math.Max(data.ArraySize / 6, 1);
            TextureCubeDX11 tex = new TextureCubeDX11(this, data.Width, data.Height, data.Flags, data.Format, data.MipMapLevels, cubeCount);
            tex.SetData(GraphicsPriority.Apply, data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        /// <summary>
        /// Resolves a source texture into a destination texture. <para/>
        /// This is most useful when re-using the resulting rendertarget of one render pass as an input to a second render pass. <para/>
        /// Another common use is transferring (resolving) a multisampled texture into a non-multisampled texture.
        /// </summary>
        /// <param name="source">The source texture.</param>
        /// <param name="destination">The destination texture.</param>
        public override void ResolveTexture(ITexture source, ITexture destination)
        {
            if (source.DataFormat != destination.DataFormat)
                throw new Exception("The source and destination texture must be the same format.");

            uint arrayLevels = Math.Min(source.ArraySize, destination.ArraySize);
            uint mipLevels = Math.Min(source.MipMapCount, destination.MipMapCount);

            for (uint i = 0; i < arrayLevels; i++)
            {
                for (uint j = 0; j < mipLevels; j++)
                {
                    TextureResolve task = TextureResolve.Get();
                    task.Source = source as TextureDX11;
                    task.Destination = destination as TextureDX11;
                    task.SourceMipLevel = j;
                    task.SourceArraySlice = i;
                    task.DestMipLevel = j;
                    task.DestArraySlice = i;
                    Renderer.PushTask(RenderTaskPriority.StartOfFrame, task);
                }
            }
        }

        /// <summary>Resources the specified sub-resource of a source texture into the sub-resource of a destination texture.</summary>
        /// <param name="source">The source texture.</param>
        /// <param name="destination">The destination texture.</param>
        /// <param name="sourceMipLevel">The source mip-map level.</param>
        /// <param name="sourceArraySlice">The source array slice.</param>
        /// <param name="destMiplevel">The destination mip-map level.</param>
        /// <param name="destArraySlice">The destination array slice.</param>
        public override void ResolveTexture(ITexture source, ITexture destination,
            uint sourceMipLevel,
            uint sourceArraySlice,
            uint destMiplevel,
            uint destArraySlice)
        {
            if (source.DataFormat != destination.DataFormat)
                throw new Exception("The source and destination texture must be the same format.");

            TextureResolve task = TextureResolve.Get();
            task.Source = source as TextureDX11;
            task.Destination = destination as TextureDX11;
            Renderer.PushTask(RenderTaskPriority.StartOfFrame, task);
        }

        protected override ShaderSampler OnCreateSampler(ref ShaderSamplerParameters parameters)
        {
            return new ShaderSamplerDX11(this, ref parameters);
        }

        public unsafe override GraphicsBuffer CreateVertexBuffer<T>(GraphicsResourceFlags flags, uint numVertices, T[] initialData = null)
        {
            uint numBytes = numVertices * (uint)sizeof(T);
            fixed (T* ptr = initialData)
                return new VertexBufferDX11<T>(this, flags | GraphicsResourceFlags.NoShaderAccess, numVertices, ptr, numBytes);
        }

        public unsafe override GraphicsBuffer CreateIndexBuffer(GraphicsResourceFlags flags, uint numIndices, ushort[] initialData = null)
        {
            uint numBytes = numIndices * sizeof(ushort);
            fixed (ushort* ptr = initialData)
                return new IndexBufferDX11(this, flags | GraphicsResourceFlags.NoShaderAccess, IndexBufferFormat.UInt16, numIndices, ptr, numIndices);
        }

        public unsafe override GraphicsBuffer CreateIndexBuffer(GraphicsResourceFlags flags, uint numIndices, uint[] initialData = null)
        {
            uint numBytes = numIndices * sizeof(uint);
            fixed (uint* ptr = initialData)
                return new IndexBufferDX11(this, flags | GraphicsResourceFlags.NoShaderAccess, IndexBufferFormat.UInt32, numIndices, ptr, numIndices);
        }

        public unsafe override GraphicsBuffer CreateStructuredBuffer<T>(GraphicsResourceFlags flags, uint numElements, T[] initialData = null)
        {
            uint numBytes = numElements * sizeof(uint);
            fixed (T* ptr = initialData)
                return new BufferDX11(this, GraphicsBufferType.Structured, flags, (uint)sizeof(T), numElements, ptr, numBytes);
        }

        public override GraphicsBuffer CreateStagingBuffer(bool allowRead, bool allowWrite, uint byteCapacity)
        {
            GraphicsResourceFlags flags = GraphicsResourceFlags.None;
            if (allowRead)
                flags |= GraphicsResourceFlags.CpuRead;

            if (allowWrite)
                flags |= GraphicsResourceFlags.CpuWrite;

            return new BufferDX11(this, GraphicsBufferType.Staging, flags | GraphicsResourceFlags.GpuWrite | GraphicsResourceFlags.NoShaderAccess, 1, byteCapacity, null, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ID3D11Device5(DeviceDX11 device)
        {
            return *device._native;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ID3D11Device5*(DeviceDX11 device)
        {
            return device._native;
        }

        /// <summary>
        /// The underlying, native device pointer.
        /// </summary>
        internal ID3D11Device5* Ptr => _native;

        /// <summary>
        /// Gets a protected reference to the underlying device pointer.
        /// </summary>
        protected ref ID3D11Device5* PtrRef => ref _native;

        internal VertexFormatCache<ShaderIOStructureDX11> VertexFormatCache { get; }

        /// <inheritdoc/>
        public override GraphicsQueueDX11 Queue => _queue;
    }
}
