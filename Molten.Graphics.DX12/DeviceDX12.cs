using Molten.Collections;
using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;
using Feature = Silk.NET.Direct3D12.Feature;

namespace Molten.Graphics.DX12;

public unsafe class DeviceDX12 : DeviceDXGI
{
    ID3D12Device10* _handle;
    DeviceBuilderDX12 _builder;
    CommandQueueDX12 _cmdDirect;
    ID3D12InfoQueue1* _debugInfo;
    uint _debugCookieID;
    ShaderLayoutCache<ShaderIOLayoutDX12> _shaderLayoutCache;
    DescriptorHeapManagerDX12 _heapManager;
    ResourceManagerDX12 _resources;
    PipelineStateBuilderDX12 _stateBuilder;
    ThreadedList<PipelineInputLayoutDX12> _pipelineLayoutCache;
    uint _nodeCount;

    GpuFrameBuffer<CommandAllocatorDX12> _cmdAllocators;
    FenceDX12 _presentFence;

    internal DeviceDX12(RendererDX12 renderer, GraphicsManagerDXGI manager, IDXGIAdapter4* adapter, DeviceBuilderDX12 deviceBuilder) :
        base(renderer, manager, adapter)
    {
        Renderer = renderer;
        _builder = deviceBuilder;
        _shaderLayoutCache = new ShaderLayoutCache<ShaderIOLayoutDX12>();
        _pipelineLayoutCache = new ThreadedList<PipelineInputLayoutDX12>();
        CapabilitiesDX12 = new CapabilitiesDX12();
    }

    internal ProtectedSessionDX12 CreateProtectedSession()
    {
        return new ProtectedSessionDX12(this);
    }

    protected override bool OnInitialize()
    {
        HResult r = _builder.CreateDevice(this, out PtrRef);
        if (!Renderer.Log.CheckResult(r, () => $"Failed to initialize {nameof(DeviceDX12)}"))
            return false;

        _nodeCount = _handle->GetNodeCount();

        // Now we need to retrieve a debug info queue from the device.
        if (Settings.EnableDebugLayer)
        {
            void* ptr = null;
            Guid guidDebugInfo = ID3D12InfoQueue1.Guid;
            _handle->QueryInterface(&guidDebugInfo, &ptr);
            _debugInfo = (ID3D12InfoQueue1*)ptr;
            _debugInfo->PushEmptyStorageFilter();

            uint debugCookieID = 0;
            r = _debugInfo->RegisterMessageCallback(new PfnMessageFunc(ProcessDebugMessage), MessageCallbackFlags.FlagNone, null, &debugCookieID);
            _debugCookieID = debugCookieID;

            if (!r.IsSuccess)
                Log.Error("Failed to register debug callback");
        }

        CommandQueueDesc cmdDesc = new()
        {
            Type = CommandListType.Direct,
            Flags = CommandQueueFlags.None
        };

        _heapManager = new DescriptorHeapManagerDX12(this);
        _stateBuilder = new PipelineStateBuilderDX12(this);
        _cmdDirect = new CommandQueueDX12(Log, this, _builder, ref cmdDesc);
        _presentFence = new FenceDX12(this, FenceFlags.None);
        _resources = new ResourceManagerDX12(this);
        _cmdAllocators = new GpuFrameBuffer<CommandAllocatorDX12>(this, (device) => new CommandAllocatorDX12(this, CommandListType.Direct));
        CommandAllocator = _cmdAllocators.Prepare();

        return true;
    }

    public override GpuFormatSupportFlags GetFormatSupport(GpuResourceFormat format)
    {
        uint sizeOf = (uint)sizeof(FeatureDataFormatSupport);
        void* pData = null;

        HResult r = _handle->CheckFeatureSupport(Feature.FormatSupport, pData, sizeOf);
        if (!Log.CheckResult(r))
        {
            Log.Error($"Failed to retrieve format '{format}' support. Code: {r}");
            return GpuFormatSupportFlags.None;
        }

        FeatureDataFormatSupport* supportData = (FeatureDataFormatSupport*)pData;
        return (GpuFormatSupportFlags)supportData->Support1;
    }

    private void ProcessDebugMessage(MessageCategory category, MessageSeverity severity, MessageID id, byte* pDescription, void* prContext)
    {
        string desc = SilkMarshal.PtrToString((nint)pDescription, NativeStringEncoding.LPStr);
        string msg = $"[DX12] [Frame {Renderer.FrameID}] [{severity} - {category}] {desc}";

        switch (severity)
        {
            case MessageSeverity.Corruption:
            case MessageSeverity.Error:
                Log.Error(msg);
                break;

            case MessageSeverity.Warning:
                Log.Warning(msg);
                break;

            case MessageSeverity.Info:
                Log.WriteLine(msg);
                break;

            default:
            case MessageSeverity.Message:
                Log.Write(msg);
                break;
        }
    }

    protected override void OnDispose(bool immediate)
    {
        _cmdAllocators?.Dispose(true);
        _presentFence?.Dispose(true);
        _cmdDirect?.Dispose(true);
        _heapManager?.Dispose(true);

        if (_debugInfo != null)
        {
            _debugInfo->UnregisterMessageCallback(_debugCookieID);
            NativeUtil.ReleasePtr(ref _debugInfo);
        }
        base.OnDispose(immediate);
    }

    protected override void OnBeginFrame(IReadOnlyThreadedList<ISwapChainSurface> surfaces)
    {
        CommandAllocator = _cmdAllocators.Prepare();
    }

    protected override void OnEndFrame(IReadOnlyThreadedList<ISwapChainSurface> surfaces)
    {
        surfaces.For(0, (index, surface) =>
        {
            if (surface.IsEnabled)
                (surface as SwapChainSurfaceDX12).Present();
        });

        Queue.Wait(_presentFence);
    }

    /// <summary>
    /// The underlying, native device pointer.
    /// </summary>
    internal ID3D12Device10* Handle => _handle;

    /// <summary>
    /// Gets a protected reference to the underlying device pointer.
    /// </summary>
    protected ref ID3D12Device10* PtrRef => ref _handle;

    public override CommandQueueDX12 Queue => _cmdDirect;

    /// <summary>
    /// Gets DirectX 12-specific capabilities.
    /// </summary>
    internal CapabilitiesDX12 CapabilitiesDX12 { get; }

    /// <inheritdoc/>
    public new RendererDX12 Renderer { get; }

    /// <inheritdoc/>
    public override ShaderLayoutCache LayoutCache => _shaderLayoutCache;

    /// <summary>
    /// Gets the number of GPU nodes in the current <see cref="DeviceDX12"/>.
    /// <para>For more info on GPU nodes, 
    /// see: https://ubm-twvideo01.s3.amazonaws.com/o1/vault/gdc2016/Presentations/Juha_Sjoholm_DX12_Explicit_Multi_GPU.pdf</para>
    /// </summary>
    internal uint NodeCount => _nodeCount;

    /// <summary>
    /// Gets the descriptor heap manager for the current <see cref="DeviceDX12"/> instance.
    /// </summary>
    internal DescriptorHeapManagerDX12 Heap => _heapManager;

    /// <inheritdoc/>
    public override GpuResourceManager Resources => _resources;

    internal PipelineStateBuilderDX12 StateBuilder => _stateBuilder;

    internal ThreadedList<PipelineInputLayoutDX12> PipelineLayoutCache => _pipelineLayoutCache;

    internal CommandAllocatorDX12 CommandAllocator { get; private set; }
}
