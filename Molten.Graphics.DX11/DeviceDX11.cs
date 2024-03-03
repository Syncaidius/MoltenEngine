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
    ID3D11Device5* _handle;
    DeviceBuilderDX11 _builder;
    GraphicsManagerDXGI _displayManager;
    ResourceManagerDX11 _resources;

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
            Handle->QueryInterface(&guidDebug, &ptr);
            _debug = (ID3D11Debug*)ptr;

            Guid guidDebugInfo = ID3D11InfoQueue.Guid;
            _debug->QueryInterface(&guidDebugInfo, &ptr);
            _debugInfo = (ID3D11InfoQueue*)ptr;
            _debugInfo->PushEmptyStorageFilter();
        }

        _resources = new ResourceManagerDX11(this);
        _queue = new GraphicsQueueDX11(this, deviceContext);
        return true;
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
        _handle->CreateDeferredContext3(0, &dc);

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
    protected override void OnDispose(bool immediate)
    {
        _queue?.Dispose();

        // TODO dispose of all bound IGraphicsResource
        LayoutCache?.Dispose();

        if (_debug != null)
        {
            ProcessDebugLayerMessages();
            _debug->ReportLiveDeviceObjects(RldoFlags.Detail);
            NativeUtil.ReleasePtr(ref _debugInfo);
            NativeUtil.ReleasePtr(ref _debug);
        }

        base.OnDispose(immediate);
    }

    public override GraphicsFormatSupportFlags GetFormatSupport(GraphicsFormat format)
    {
        uint value = 0;
        HResult hr = _handle->CheckFormatSupport((Format)format, &value);
        if (!Log.CheckResult(hr, () => "Failed to create pipeline state object (PSO)"))
            return GraphicsFormatSupportFlags.None;

        return (GraphicsFormatSupportFlags)value;
    }

    protected override void OnBeginFrame(IReadOnlyThreadedList<ISwapChainSurface> surfaces)
    {
        
    }

    protected override void OnEndFrame(IReadOnlyThreadedList<ISwapChainSurface> surfaces)
    {
        surfaces.For(0, (index, surface) =>
        {
            if (surface.IsEnabled)
                (surface as SwapChainSurfaceDX11).Present();
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ID3D11Device5(DeviceDX11 device)
    {
        return *device._handle;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ID3D11Device5*(DeviceDX11 device)
    {
        return device._handle;
    }

    /// <summary>
    /// The underlying, native device pointer.
    /// </summary>
    internal ID3D11Device5* Handle => _handle;

    /// <summary>
    /// Gets a protected reference to the underlying device pointer.
    /// </summary>
    protected ref ID3D11Device5* PtrRef => ref _handle;

    /// <inheritdoc/>
    public override GraphicsQueueDX11 Queue => _queue;

    /// <inheritdoc/>
    public override ShaderLayoutCache LayoutCache => _layoutCache;

    public override ResourceManagerDX11 Resources => _resources;
}
