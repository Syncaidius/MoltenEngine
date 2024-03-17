using Molten.Cache;
using Molten.Collections;

namespace Molten.Graphics;

/// <summary>
/// The base class for an API-specific implementation of a graphics device, which provides command/resource access to a GPU.
/// </summary>
public abstract partial class GpuDevice : EngineObject
{
    public delegate void FrameBufferSizeChangedHandler(uint oldSize, uint newSize);

    /// <summary>Occurs when a connected <see cref="IDisplayOutput"/> is activated on the current <see cref="GpuDevice"/>.</summary>
    public event DisplayOutputChanged OnOutputActivated;

    /// <summary>Occurs when a connected <see cref="IDisplayOutput"/> is deactivated on the current <see cref="GpuDevice"/>.</summary>
    public event DisplayOutputChanged OnOutputDeactivated;

    /// <summary>
    /// Invoked when the frame-buffer size is changed for the current <see cref="GpuDevice"/>.
    /// </summary>
    public event FrameBufferSizeChangedHandler OnFrameBufferSizeChanged;

    const int INITIAL_BRANCH_COUNT = 3;

    long _allocatedVRAM;
    GpuFrame[] _frames;
    uint _frameIndex;
    uint _newFrameBufferSize;
    uint _maxStagingSize;

    ThreadedList<GpuObject> _disposals;

    /// <summary>
    /// Creates a new instance of <see cref="GpuDevice"/>.
    /// </summary>
    /// <param name="renderer">The <see cref="RenderService"/> that the new graphics device will be bound to.</param>
    /// <param name="manager">The <see cref="GpuManager"/> that the device will be bound to.</param>
    protected GpuDevice(RenderService renderer, GpuManager manager)
    {
        Settings = renderer.Settings.Graphics;
        Renderer = renderer;
        Manager = manager;
        Log = renderer.Log;
        Profiler = new GraphicsDeviceProfiler();
        Tasks = new GpuTaskManager(this);

        Cache = new ObjectCache();
        _disposals = new ThreadedList<GpuObject>();
        _maxStagingSize = (uint)ByteMath.FromMegabytes(renderer.Settings.Graphics.FrameStagingSize);

        SettingValue<FrameBufferMode> bufferingMode = renderer.Settings.Graphics.FrameBufferMode;
        BufferingMode_OnChanged(bufferingMode.Value, bufferingMode.Value);
        bufferingMode.OnChanged += BufferingMode_OnChanged;
    }

    /// <summary>
    /// Gets whether or not the provided <see cref="GpuFormatSupportFlags"/> are supported
    /// by the current <see cref="GpuDevice"/> for the specified <see cref="GpuResourceFormat"/>.
    /// </summary>
    /// <param name="format">The <see cref="GpuResourceFormat"/> to check for support.</param>
    /// <param name="flags">The support flags to be checked.</param>
    /// <returns></returns>
    public bool IsFormatSupported(GpuResourceFormat format, GpuFormatSupportFlags flags)
    {
        if(flags == GpuFormatSupportFlags.None)
            throw new Exception("Cannot check for support with no flags.");

        GpuFormatSupportFlags support = GetFormatSupport(format);
        return (support & flags) == flags;
    }

    public abstract GpuFormatSupportFlags GetFormatSupport(GpuResourceFormat format);

    private void BufferingMode_OnChanged(FrameBufferMode oldValue, FrameBufferMode newValue)
    {
        SettingValue<FrameBufferMode> bufferingMode = Settings.FrameBufferMode;

        // Does the buffer mode exceed the minimum?
        _newFrameBufferSize = bufferingMode.Value switch
        {
            FrameBufferMode.Triple => 3,
            FrameBufferMode.Quad => 4,
            _ => 2,
        };
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
    /// Activates a <see cref="IDisplayOutput"/> on the current <see cref="GpuDevice"/>.
    /// </summary>
    /// <param name="output">The output to be activated.</param>
    public abstract void AddActiveOutput(IDisplayOutput output);

    /// <summary>
    /// Deactivates a <see cref="IDisplayOutput"/> from the current <see cref="GpuDevice"/>. It will still be listed in <see cref="Outputs"/>, if attached.
    /// </summary>
    /// <param name="output">The output to be deactivated.</param>
    public abstract void RemoveActiveOutput(IDisplayOutput output);

    /// <summary>
    /// Removes all active <see cref="IDisplayOutput"/> from the current <see cref="GpuDevice"/>. They will still be listed in <see cref="Outputs"/>, if attached.
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

    internal void MarkForRelease(GpuObject obj)
    {
        if (IsDisposed)
            throw new ObjectDisposedException("GraphicsDevice has already been disposed, so it cannot mark GraphicsObject instances for release.");

        obj.ReleaseFrameID = Renderer.FrameID;
        _disposals.Add(obj);
    }

    protected override void OnDispose(bool immediate)
    {
        Tasks?.Dispose();

        if (_frames != null)
        {
            for (int i = 0; i < _frames.Length; i++)
                _frames[i].Dispose();
        }

        // Dispose of any registered output services.
        Resources.OutputSurfaces.For(0, (index, surface) =>
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

        CheckFrameBufferSize(false);

        if (OnInitialize())
        {
            IsInitialized = true;
            CheckFrameBufferSize(true);
        }
        else
        {
            Log.Error($"Failed to initialize {this.Name}");
        }

        return IsInitialized;
    }

    protected abstract bool OnInitialize();

    private void CheckFrameBufferSize(bool checkStagingBuffers)
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
                if (checkStagingBuffers)
                {
                    for (int i = 0; i < FrameBufferSize; i++)
                    {
                        _frames[i] ??= new GpuFrame(INITIAL_BRANCH_COUNT);
                        _frames[i].StagingBuffer = Resources.CreateStagingBuffer(true, true, _maxStagingSize);
                    }
                }
                else
                {
                    for (int i = 0; i < FrameBufferSize; i++)
                        _frames[i] ??= new GpuFrame(INITIAL_BRANCH_COUNT);
                }
            }
        }
    }

    internal void BeginFrame()
    {
        OnBeginFrame(Resources.OutputSurfaces);

        // TODO check if _maxStagingSize has changed due to settings. May need to resize all existing staging buffers.
        CheckFrameBufferSize(true);

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

    internal void EndFrame(Timing time)
    {
        OnEndFrame(Resources.OutputSurfaces);

        _frames[_frameIndex].FrameID = Renderer.FrameID;
        _frameIndex = (_frameIndex + 1U) % FrameBufferSize;
    }

    protected abstract void OnBeginFrame(IReadOnlyThreadedList<ISwapChainSurface> surfaces);

    protected abstract void OnEndFrame(IReadOnlyThreadedList<ISwapChainSurface> surfaces);

    /// <summary>
    /// Gets the amount of VRAM that has been allocated on the current <see cref="GpuDevice"/>. 
    /// <para>For a software or integration device, this may be system memory (RAM).</para>
    /// </summary>
    internal long AllocatedVRAM => _allocatedVRAM;

    /// <summary>
    /// Gets the current frame on the current <see cref="GpuDevice"/>.
    /// </summary>
    public GpuFrame Frame => _frames[_frameIndex];

    /// <summary>
    /// Gets the <see cref="Logger"/> that is bound to the current <see cref="GpuDevice"/> for outputting information.
    /// </summary>
    public Logger Log { get; }

    /// <summary>
    /// Gets the <see cref="GraphicsSettings"/> bound to the current <see cref="GpuDevice"/>.
    /// </summary>
    public GraphicsSettings Settings { get; }

    /// <summary>
    /// Gets the <see cref="GpuManager"/> that owns the current <see cref="GpuDevice"/>.
    /// </summary>
    public GpuManager Manager { get; }

    /// <summary>
    /// The main <see cref="GpuCommandQueue"/> of the current <see cref="GpuDevice"/>. This is used for issuing immediate commands to the GPU.
    /// </summary>
    public abstract GpuCommandQueue Queue { get; }

    /// <summary>
    /// Gets the <see cref="RenderService"/> that created and owns the current <see cref="GpuDevice"/> instance.
    /// </summary>
    public RenderService Renderer { get; }

    /// <summary>Gets the machine-local device ID of the current <see cref="GpuDevice"/>.</summary>
    public abstract DeviceID ID { get; }

    /// <summary>The hardware vendor.</summary>
    public abstract DeviceVendor Vendor { get; }

    /// <summary>
    /// Gets the <see cref="GpuDeviceType"/> of the current <see cref="GpuDevice"/>.
    /// </summary>
    public abstract GpuDeviceType Type { get; }

    /// <summary>Gets a list of all <see cref="IDisplayOutput"/> devices attached to the current <see cref="GpuDevice"/>.</summary>
    public abstract IReadOnlyList<IDisplayOutput> Outputs { get; }

    /// <summary>Gets a list of all active <see cref="IDisplayOutput"/> devices attached to the current <see cref="GpuDevice"/>.
    /// <para>Active outputs are added via <see cref="AddActiveOutput(IDisplayOutput)"/>.</para></summary>
    public abstract IReadOnlyList<IDisplayOutput> ActiveOutputs { get; }

    /// <summary>
    /// Gets the capabilities of the current <see cref="GpuDevice"/>.
    /// </summary>
    public GpuCapabilities Capabilities { get; protected set; }

    /// <summary>
    /// Gets the vertex format cache which stores <see cref="ShaderIOLayout"/> instances to help avoid the need to generate multiple instances of the same formats.
    /// </summary>
    public abstract ShaderLayoutCache LayoutCache { get; }

    /// <summary>
    /// Gets the profiler attached to the current device.
    /// </summary>
    public GraphicsDeviceProfiler Profiler { get; } 

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
    /// Gets whether or not the current <see cref="GpuDevice"/> is initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Gets the <see cref="ObjectCache"/> that is bound to the current <see cref="GpuDevice"/>.
    /// </summary>
    public ObjectCache Cache { get; }

    /// <summary>
    /// Gets the task manager of the current <see cref="GpuDevice"/>.
    /// </summary>
    public GpuTaskManager Tasks { get; }

    /// <summary>
    /// Gets the <see cref="GpuResourceManager"/> implementation for the current <see cref="GpuDevice"/>. 
    /// </summary>
    public abstract GpuResourceManager Resources { get; }
}
