using System.Diagnostics;
using System.Reflection;
using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Message = Silk.NET.Direct3D11.Message;

namespace Molten.Graphics
{
    /// <summary>A Direct3D 11 graphics device.</summary>
    /// <seealso cref="CommandQueueDX11" />
    public unsafe class DeviceDX11 : GraphicsDevice<ID3D11Device5>
    {
        DeviceBuilderDX11 _builder;
        DisplayAdapterDXGI _adapter;
        DisplayManagerDXGI _displayManager;

        CommandQueueDX11 CmdList;
        List<CommandQueueDX11> _deferredContexts;

        ID3D11Debug* _debug;
        ID3D11InfoQueue* _debugInfo;

        /// <summary>The adapter to initially bind the graphics device to. Can be changed later.</summary>
        /// <param name="adapter">The physical display adapter to bind the new device to.</param>
        internal DeviceDX11(RenderService renderer, GraphicsSettings settings, DeviceBuilderDX11 builder, IDisplayAdapter adapter) :
            base(renderer, settings, false)
        {
            _builder = builder;
            _displayManager = adapter.Manager as DisplayManagerDXGI;
            _adapter = adapter as DisplayAdapterDXGI;
            _deferredContexts = new List<CommandQueueDX11>();

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
                    Log.Error($"[DX11 DEBUG] [Frame {Cmd.Profiler.FrameID}] [{msg->Severity}] [{msg->Category}] {desc}");
                }

                _debugInfo->ClearStoredMessages();
            }
        }

        protected override void OnInitialize()
        {
            HResult r = _builder.CreateDevice(_adapter, out PtrRef, out ID3D11DeviceContext4* deviceContext);
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

            CmdList = new CommandQueueDX11(this, deviceContext);
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

        /// <summary>Gets a new deferred <see cref="CommandQueueDX11"/>.</summary>
        /// <returns></returns>
        internal CommandQueueDX11 GetDeferredContext()
        {
            ID3D11DeviceContext3* dc = null;
            Ptr->CreateDeferredContext3(0, &dc);

            Guid cxt4Guid = ID3D11DeviceContext4.Guid;
            void* ptr4 = null;
            dc->QueryInterface(&cxt4Guid, &ptr4);

            CommandQueueDX11 context = new CommandQueueDX11(this, (ID3D11DeviceContext4*)ptr4);
            _deferredContexts.Add(context);
            return context;
        }

        internal void RemoveDeferredContext(CommandQueueDX11 cmd)
        {
            if (cmd.DXDevice != this)
                throw new GraphicsCommandQueueException(cmd, "Graphics pipe is owned by another device.");

            if (!cmd.IsDisposed)
                cmd.Dispose();

            _deferredContexts.Remove(cmd);
        }

        internal void SubmitContext(CommandQueueDX11 context)
        {
            if (context.Type != CommandQueueType.Deferred)
                throw new Exception("Cannot submit immediate graphics contexts, only deferred.");

            // TODO take the underlying DX context from the GraphicsContext and give it a new/recycled one to work with.
            // TODO add the context's profiler stats to the device's main profiler.
        }

        /// <summary>Disposes of the <see cref="DeviceDX11"/> and any deferred contexts and resources bound to it.</summary>
        protected override void OnDispose()
        {
            for (int i = _deferredContexts.Count - 1; i >= 0; i--)
                RemoveDeferredContext(_deferredContexts[i]);

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


        public override IDepthStencilSurface CreateDepthSurface(
            uint width,
            uint height,
            DepthFormat format = DepthFormat.R24G8_Typeless,
            uint mipCount = 1,
            uint arraySize = 1,
            AntiAliasLevel aaLevel = AntiAliasLevel.None,
            TextureFlags flags = TextureFlags.None,
            string name = "surface")
        {
            MSAAQuality msaa = MSAAQuality.CenterPattern;
            return new DepthStencilSurface(Renderer, width, height, format, mipCount, arraySize, aaLevel, msaa, flags, name);
        }

        protected override HlslPass OnCreateShaderPass(HlslShader shader, string name = null)
        {
            return new ShaderPassDX11(shader, name);
        }

        public override INativeSurface CreateFormSurface(string formTitle, string formName, uint mipCount = 1)
        {
            return new RenderFormSurface(Renderer, formTitle, formName, mipCount);
        }

        public override INativeSurface CreateControlSurface(string formTitle, string controlName, uint mipCount = 1)
        {
            return new RenderControlSurface(Renderer, formTitle, controlName, mipCount);
        }

        public override IRenderSurface2D CreateSurface(
            uint width,
            uint height,
            GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm,
            uint mipCount = 1,
            uint arraySize = 1,
            AntiAliasLevel aaLevel = AntiAliasLevel.None,
            TextureFlags flags = TextureFlags.None,
            string name = null)
        {
            MSAAQuality msaa = MSAAQuality.CenterPattern;
            return new RenderSurface2D(Renderer, width, height, (Format)format, mipCount, arraySize, aaLevel, msaa, flags, name);
        }

        public override ITexture CreateTexture1D(Texture1DProperties properties)
        {
            return new Texture1D(Renderer, properties.Width, properties.Format.ToApi(), properties.MipMapLevels, properties.ArraySize, properties.Flags);
        }

        public override ITexture CreateTexture1D(TextureData data)
        {
            Texture1D tex = new Texture1D(Renderer, data.Width, data.Format.ToApi(), data.MipMapLevels, data.ArraySize, data.Flags);
            tex.SetData(data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        public override ITexture2D CreateTexture2D(Texture2DProperties properties)
        {
            return new Texture2D(Renderer,
                properties.Width,
                properties.Height,
                properties.Format.ToApi(),
                properties.MipMapLevels,
                properties.ArraySize,
                properties.Flags,
                properties.MultiSampleLevel,
                properties.SampleQuality,
                properties.Name);
        }

        public override ITexture2D CreateTexture2D(TextureData data)
        {
            Texture2D tex = new Texture2D(Renderer,
                data.Width,
                data.Height,
                data.Format.ToApi(),
                data.MipMapLevels,
                data.ArraySize,
                data.Flags,
                data.MultiSampleLevel);

            tex.SetData(data, 0, 0, data.MipMapLevels, data.ArraySize);
            return tex;
        }

        public override ITexture3D CreateTexture3D(Texture3DProperties properties)
        {
            return new Texture3D(Renderer,
                properties.Width,
                properties.Height,
                properties.Depth,
                properties.Format.ToApi(),
                properties.MipMapLevels,
                properties.Flags);
        }

        public override ITexture3D CreateTexture3D(TextureData data)
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

        public override ITextureCube CreateTextureCube(Texture2DProperties properties)
        {
            uint cubeCount = Math.Max(properties.ArraySize / 6, 1);
            return new TextureCubeDX11(Renderer, properties.Width, properties.Height, properties.Format.ToApi(), properties.MipMapLevels, cubeCount, properties.Flags);
        }

        public override ITextureCube CreateTextureCube(TextureData data)
        {
            uint cubeCount = Math.Max(data.ArraySize / 6, 1);
            TextureCubeDX11 tex = new TextureCubeDX11(Renderer, data.Width, data.Height, data.Format.ToApi(), data.MipMapLevels, cubeCount, data.Flags);
            tex.SetData(data, 0, 0, data.MipMapLevels, data.ArraySize);
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
                    task.Source = source as TextureBase;
                    task.Destination = destination as TextureBase;
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
            task.Source = source as TextureBase;
            task.Destination = destination as TextureBase;
            Renderer.PushTask(RenderTaskPriority.StartOfFrame, task);
        }

        protected override ShaderSampler OnCreateSampler(ref ShaderSamplerParameters parameters)
        {
            return new ShaderSamplerDX11(this, ref parameters);
        }

        public unsafe override IVertexBuffer CreateVertexBuffer<T>(BufferFlags flags, uint numVertices, T[] initialData = null)
        {
            fixed (T* ptr = initialData)
                return new VertexBufferDX11<T>(this, flags, numVertices, ptr);
        }

        public unsafe override IIndexBuffer CreateIndexBuffer(BufferFlags flags, uint numIndices, ushort[] initialData = null)
        {
            fixed (ushort* ptr = initialData)
                return new IndexBufferDX11(this, flags, IndexBufferFormat.UInt16, numIndices, ptr);
        }

        public unsafe override IIndexBuffer CreateIndexBuffer(BufferFlags flags, uint numIndices, uint[] initialData = null)
        {
            fixed (uint* ptr = initialData)
                return new IndexBufferDX11(this, flags, IndexBufferFormat.UInt32, numIndices, ptr);
        }

        public unsafe override IStructuredBuffer CreateStructuredBuffer<T>(BufferFlags flags, uint numElements, bool allowUnorderedAccess, bool isShaderResource, T[] initialData = null)
        {
            fixed (T* ptr = initialData)
                return new StructuredBufferDX11<T>(this, flags, numElements, allowUnorderedAccess, isShaderResource, ptr);
        }

        public override IStagingBuffer CreateStagingBuffer(bool allowRead, bool allowWrite, uint byteCapacity)
        {
            BufferFlags flags = BufferFlags.None;
            if (allowRead)
                flags |= BufferFlags.CpuRead;

            if (allowWrite)
                flags |= BufferFlags.CpuWrite;

            return new StagingBuffer(this, flags, byteCapacity);
        }

        public override DisplayManagerDXGI DisplayManager => _displayManager;

        public override DisplayAdapterDXGI Adapter => _adapter;

        internal VertexFormatCache<ShaderIOStructureDX11> VertexFormatCache { get; }

        /// <inheritdoc/>
        public override CommandQueueDX11 Cmd => CmdList;
    }
}
