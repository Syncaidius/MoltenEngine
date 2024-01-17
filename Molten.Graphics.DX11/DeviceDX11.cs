using Molten.Collections;
using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System.Reflection;
using System.Runtime.CompilerServices;
using Message = Silk.NET.Direct3D11.Message;

namespace Molten.Graphics.DX11;

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
    ShaderLayoutCache<ShaderIOLayoutDX11> _layoutCache;

    /// <summary>The adapter to initially bind the graphics device to. Can be changed later.</summary>
    /// <param name="adapter">The physical display adapter to bind the new device to.</param>
    internal DeviceDX11(RenderService renderer, GraphicsManagerDXGI manager, IDXGIAdapter4* adapter, DeviceBuilderDX11 builder) :
        base(renderer, manager, adapter)
    {
        _builder = builder;
        _displayManager = manager;
        _cmdDeferred = new List<GraphicsQueueDX11>();
        _layoutCache = new ShaderLayoutCache<ShaderIOLayoutDX11>();
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
                Log.Error($"[DX11 DEBUG] [Frame {Renderer.FrameID}] [{msg->Severity}] [{msg->Category}] {desc}");
            }

            _debugInfo->ClearStoredMessages();
        }
    }

    protected override bool OnInitialize()
    {
        HResult r = _builder.CreateDevice(this, out PtrRef, out ID3D11DeviceContext4* deviceContext);

        if (r.IsFailure)
        {
            Log.Error($"Failed to initialize {nameof(DeviceDX11)}. Code: {r}");
            return false;
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
        return true;
    }

    protected override uint MinimumFrameBufferSize()
    {
        return 2;
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
        LayoutCache.Dispose();

        if (_debug != null)
        {
            ProcessDebugLayerMessages();
            _debug->ReportLiveDeviceObjects(RldoFlags.Detail);
            NativeUtil.ReleasePtr(ref _debugInfo);
            NativeUtil.ReleasePtr(ref _debug);
        }

        base.OnDispose();
    }

    protected override void OnBeginFrame(ThreadedList<ISwapChainSurface> surfaces)
    {
        
    }

    protected override void OnEndFrame(ThreadedList<ISwapChainSurface> surfaces)
    {
        surfaces.For(0, (index, surface) =>
        {
            if (surface.IsEnabled)
                (surface as SwapChainSurfaceDX11).Present();
        });
    }

    public override IDepthStencilSurface CreateDepthSurface(uint width, uint height, 
        DepthFormat format = DepthFormat.R24G8_Typeless, 
        GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite, 
        uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, string name = null)
    {
        MSAAQuality msaa = MSAAQuality.CenterPattern;
        return new DepthSurfaceDX11(this, width, height, flags, format, mipCount, arraySize, aaLevel, msaa, name);
    }

    protected override HlslPass OnCreateShaderPass(HlslShader shader, string name = null)
    {
        return new ShaderPassDX11(shader, name);
    }

    protected override INativeSurface OnCreateFormSurface(string formTitle, string formName, uint width, uint height,
        GraphicsFormat format = GraphicsFormat.B8G8R8A8_UNorm, uint mipCount = 1)
    {
        return new WindowsFormSurface(this, 800, 600, 1, formTitle, formName);
    }

    protected override INativeSurface OnCreateControlSurface(string formTitle, string controlName, uint mipCount = 1)
    {
        throw new NotImplementedException(); // return new RenderControlSurface(this, formTitle, controlName, mipCount);
    }

    public override IRenderSurface2D CreateSurface(uint width, uint height, 
        GraphicsFormat format = GraphicsFormat.R8G8B8A8_SNorm, 
        GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite, 
        uint mipCount = 1, 
        uint arraySize = 1, 
        AntiAliasLevel aaLevel = AntiAliasLevel.None, string name = null)
    {
        MSAAQuality msaa = MSAAQuality.CenterPattern;
        return new RenderSurface2DDX11(this, width, height, flags, format, mipCount, arraySize, aaLevel, msaa, name);
    }

    public override ITexture1D CreateTexture1D(uint width, uint mipCount, uint arraySize, 
        GraphicsFormat format, GraphicsResourceFlags flags, string name = null)
    {
        return new Texture1DDX11(this, width, flags, format, mipCount, arraySize, name);
    }

    public override ITexture2D CreateTexture2D(uint width, uint height, uint mipCount, uint arraySize, 
        GraphicsFormat format, GraphicsResourceFlags flags,
        AntiAliasLevel aaLevel = AntiAliasLevel.None,
        MSAAQuality aaQuality = MSAAQuality.Default, string name = null)
    {
        return new Texture2DDX11(this, width, height, flags, format, mipCount, arraySize, aaLevel, aaQuality, name);
    }

    public override ITexture3D CreateTexture3D(uint width, uint height, uint depth, uint mipCount, GraphicsFormat format, GraphicsResourceFlags flags, string name = null)
    {
        return new Texture3DDX11(this, width, height, depth, flags, format, mipCount, name);
    }

    public override ITextureCube CreateTextureCube(uint width, uint height, uint mipCount, GraphicsFormat format, 
        uint cubeCount = 1, uint arraySize = 1, GraphicsResourceFlags flags = GraphicsResourceFlags.None, string name = null)
    {
        return new TextureCubeDX11(this, width, height, flags, format, mipCount, cubeCount, name);
    }

    /// <summary>
    /// Resolves a source texture into a destination texture. <para/>
    /// This is most useful when re-using the resulting rendertarget of one render pass as an input to a second render pass. <para/>
    /// Another common use is transferring (resolving) a multisampled texture into a non-multisampled texture.
    /// </summary>
    /// <param name="source">The source texture.</param>
    /// <param name="destination">The destination texture.</param>
    public override void ResolveTexture(GraphicsTexture source, GraphicsTexture destination)
    {
        if (source.ResourceFormat != destination.ResourceFormat)
            throw new Exception("The source and destination texture must be the same format.");

        uint arrayLevels = Math.Min(source.ArraySize, destination.ArraySize);
        uint mipLevels = Math.Min(source.MipMapCount, destination.MipMapCount);

        for (uint i = 0; i < arrayLevels; i++)
        {
            for (uint j = 0; j < mipLevels; j++)
            {
                TextureResolve task = Tasks.Get<TextureResolve>();
                task.Destination = destination as TextureDX11;
                task.SourceMipLevel = j;
                task.SourceArraySlice = i;
                task.DestMipLevel = j;
                task.DestArraySlice = i;
                Tasks.Push(GraphicsPriority.StartOfFrame, source as TextureDX11, task);
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
    public override void ResolveTexture(GraphicsTexture source, GraphicsTexture destination,
        uint sourceMipLevel,
        uint sourceArraySlice,
        uint destMiplevel,
        uint destArraySlice)
    {
        if (source.ResourceFormat != destination.ResourceFormat)
            throw new Exception("The source and destination texture must be the same format.");

        TextureResolve task = Tasks.Get<TextureResolve>();
        task.Destination = destination as TextureDX11;
        Tasks.Push(GraphicsPriority.StartOfFrame, source as TextureDX11, task);
    }

    protected override ShaderSampler OnCreateSampler(ref ShaderSamplerParameters parameters)
    {
        return new SamplerDX11(this, ref parameters);
    }

    protected override GraphicsBuffer CreateBuffer<T>(GraphicsBufferType type, GraphicsResourceFlags flags, GraphicsFormat format, uint numElements, T[] initialData)
    {
        uint stride = (uint)sizeof(T);
        uint initialBytes = initialData != null ? (uint)initialData.Length * stride : 0;

        fixed(T* ptrData = initialData)
            return new BufferDX11(this, type, flags, format, stride, numElements, 1, ptrData, initialBytes);
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

    /// <inheritdoc/>
    public override GraphicsQueueDX11 Queue => _queue;

    public override ShaderLayoutCache LayoutCache => _layoutCache;
}
