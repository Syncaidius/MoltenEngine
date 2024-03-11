using Molten.Collections;
using Molten.Graphics.Overlays;

namespace Molten.Graphics;

/// <summary>
/// A base class that custom renderer implementations must inherit in order to be compatible with Molten engine, 
/// as it provides basic functionality for interacting with the rest of the engine.
/// </summary>
public abstract class RenderService : EngineService
{
    bool _disposeRequested;
    bool _shouldPresent;
    bool _surfaceResizeRequired;

    RenderChain _chain;
    List<GpuDevice> _devices;

    AntiAliasLevel _requestedMultiSampleLevel = AntiAliasLevel.None;

    internal AntiAliasLevel MsaaLevel = AntiAliasLevel.None;
    internal Shader FxStandardMesh;
    internal Shader FxStandardMesh_NoNormalMap;

    /// <summary>
    /// Creates a new instance of a <see cref="RenderService"/> sub-type.
    /// </summary>
    public RenderService()
    {
        _devices = new List<GpuDevice>();

        Surfaces = new SurfaceManager(this);
        Overlay = new OverlayProvider();
    }

    protected override void OnStart(EngineSettings settings)
    {
        _shouldPresent = true;
        ChangeTargetFPS(0, settings.Graphics.TargetFPS.Value);
        Profiler = new GraphicsProfiler(Thread.Timing, 60);
        settings.Graphics.TargetFPS.OnChanged += ChangeTargetFPS;
    }

    private void ChangeTargetFPS(int oldValue, int newValue)
    {
        Thread.Timing.TargetUPS = Math.Max(1, newValue);
    }

    protected override void OnStop(EngineSettings settings)
    {
        _shouldPresent = false;
    }

    /// <summary>
    /// Occurs when the renderer is being initialized.
    /// </summary>
    /// <param name="settings">The engine settings to apply and bind to the current <see cref="RenderService"/>.</param>
    protected override sealed void OnInitialize(EngineSettings settings)
    {
        DisplayManager = OnInitializeDisplayManager(settings.Graphics);
        _chain = new RenderChain(this);

        try
        {
            DisplayManager.Initialize(this, settings.Graphics);
            Log.WriteLine($"Initialized display manager");
        }
        catch (Exception ex)
        {
            Log.Error("Failed to initialize renderer");
            Log.Error(ex, true);
        }

        settings.Graphics.Log(Log, "Graphics");
        MsaaLevel = _requestedMultiSampleLevel = MsaaLevel;
        settings.Graphics.MSAA.OnChanged += MSAA_OnChanged;

        try
        {
            List<GpuDevice> devices = OnInitializeDevices(settings.Graphics, DisplayManager);
            _devices.AddRange(devices);

            Device = _devices[0];

            Log.WriteLine($"Initialized {_devices.Count} GPU(s):");
            Log.WriteLine($"   Primary: {Device.Name}");
            for(int i = 1; i < _devices.Count; i++)
                Log.WriteLine($"   Secondary: {_devices[i].Name}"); 
        }
        catch (Exception ex)
        {
            Log.Error("Failed to initialize GPU");
            Log.Error(ex, true);
        }

        OnInitializeRenderer(settings);

        SpriteBatch = new SpriteBatcher(Device, 3000, 20);
        LoadDefaultShaders();

        Surfaces.Initialize(BiggestWidth, BiggestHeight);
        Fonts = new SpriteFontManager(Log, this);
        Fonts.Initialize();
    }

    /// <summary>
    /// Present's the renderer to it's bound output devices/surfaces.
    /// </summary>
    /// <param name="time"></param>
    protected override sealed void OnUpdate(Timing time)
    {
        if (_disposeRequested)
        {
            Surfaces.Dispose();
            DisposeBeforeRender();
            return;
        }

        if (!_shouldPresent)
            return;

        Device.BeginFrame();

        // Handle any pending graphics-based disposals.
        Timing timing = Thread.Timing;
        uint framesToWait = (uint)timing.TargetUPS / 4U;
        Device.DisposeMarkedObjects(framesToWait, timing.FrameID);

        if (_requestedMultiSampleLevel != MsaaLevel)
        {
            // TODO re-create all internal surfaces/textures to match the new sample level.
            // TODO adjust rasterizer mode accordingly (multisample enabled/disabled).
            MsaaLevel = _requestedMultiSampleLevel;
            _surfaceResizeRequired = true;
        }

        // Process the start-of-frame task queue for each device.
        for (int i = 0; i < _devices.Count; i++)
            _devices[i].Tasks.Process(GraphicsTaskPriority.StartOfFrame);

        // Perform preliminary checks on active scene data.
        // Also ensure the backbuffer is always big enough for the largest scene render surface.
        SceneRenderData data;
        RenderCamera camera;
        GpuDevice device;

        for (int i =0; i < Scenes.Count; i++)
        {
            data = Scenes[i];

            for(int c = 0; c < data.Cameras.Count; c++)
            {
                camera = data.Cameras[c];
                camera.Skip = false;

                if (camera.Surface == null)
                {
                    camera.Skip = true;
                    continue;
                }

                if (camera.Surface.Width > BiggestWidth)
                {
                    _surfaceResizeRequired = true;
                    BiggestWidth = camera.Surface.Width;
                }

                if (camera.Surface.Height > BiggestHeight)
                {
                    _surfaceResizeRequired = true;
                    BiggestHeight = camera.Surface.Height;
                }
            }
        }

        // Update surfaces if dirty. This may involve resizing or changing their format.
        if (_surfaceResizeRequired)
        {
            Surfaces.Rebuild(BiggestWidth, BiggestHeight);
            _surfaceResizeRequired = false;
        }

        for (int i = 0; i < Scenes.Count; i++)
        {
            data = Scenes[i];

            if (!data.IsVisible)
                continue;

            Device.Queue.BeginEvent("Draw Scene");
            data.PreRenderInvoke(this);

            // Sort cameras into ascending order-depth.
            data.Cameras.Sort((a, b) =>
            {
                if (a.OrderDepth > b.OrderDepth)
                    return 1;
                else if (a.OrderDepth < b.OrderDepth)
                    return -1;
                else
                    return 0;
            });

            for (int c = 0; c < data.Cameras.Count; c++)
            {
                camera = data.Cameras[c];

                if (camera.Skip)
                    continue;

                _chain.Render(Device.Queue, data, camera, time);
            }

            data.PostRenderInvoke(this);
            Device.Queue.EndEvent();
        }

        for (int i = 0; i < _devices.Count; i++)
            _devices[i].Tasks.Process(GraphicsTaskPriority.EndOfFrame);
        Device.EndFrame(time);
        Surfaces.ResetFirstCleared();

        // Accumulate profiling information.
        for(int i = 0; i < _devices.Count; i++)
        {
            device = _devices[i];
            device.Profiler.Accumulate(device.Queue.Profiler);
            Profiler.Accumulate(device.Profiler);
        }
    }

    internal void RenderSceneLayer(GpuCommandList cmd, LayerRenderData layerData, RenderCamera camera)
    {
        // TODO To start with we're just going to draw ALL objects in the render tree.
        // Sorting and culling will come later

        foreach (KeyValuePair<Renderable, RenderDataBatch> p in layerData.Renderables)
        {
            // Update transforms.
            // TODO replace below with render prediction to interpolate between the current and target transform.
            foreach (ObjectRenderData data in p.Value.Data)
                data.RenderTransform = data.TargetTransform;

            // If batch rendering isn't supported, render individually.
            if (!p.Key.BatchRender(cmd, this, camera, p.Value))
            {
                foreach (ObjectRenderData data in p.Value.Data)
                    p.Key.Render(cmd, this, camera, data);
            }
        }
    }

    private void LoadDefaultShaders()
    {
        ShaderCompileResult result = Device.Resources.LoadEmbeddedShader("Molten.Assets", "gbuffer.json");
        FxStandardMesh = result["gbuffer"];
        FxStandardMesh_NoNormalMap = result["gbuffer-sans-nmap"];
    }

    private void MSAA_OnChanged(AntiAliasLevel oldValue, AntiAliasLevel newValue)
    {
        _requestedMultiSampleLevel = newValue;
    }

    internal SceneRenderData CreateRenderData()
    {
        SceneRenderData rd = new SceneRenderData(Device.Tasks);
        RenderAddScene task = Device.Tasks.Get<RenderAddScene>();
        task.Data = rd;

        Device.Tasks.Push(GraphicsTaskPriority.StartOfFrame, task);
        return rd;
    }

    public void DestroyRenderData(SceneRenderData data)
    {
        RenderRemoveScene task = Device.Tasks.Get<RenderRemoveScene>();
        task.Data = data;

        Device.Tasks.Push(GraphicsTaskPriority.StartOfFrame, task);
    }

    /// <summary>
    /// Invoked when the renderer
    /// </summary>
    /// <param name="settings"></param>
    protected abstract void OnInitializeRenderer(EngineSettings settings);

    /// <summary>
    /// Invoked during the first stage of service initialization to allow any api-related objects to be created/initialized prior to renderer initialization.
    /// </summary>
    /// <param name="settings">The <see cref="GraphicsSettings"/> bound to the current engine instance.</param>
    protected abstract GpuManager OnInitializeDisplayManager(GraphicsSettings settings);

    /// <summary>
    /// Invoked during render service initialization to allow the provisioning and initialization of any available/supported graphics devices on the host system.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="manager"></param>
    /// <returns></returns>
    protected abstract List<GpuDevice> OnInitializeDevices(GraphicsSettings settings, GpuManager manager);

    /// <summary>
    /// Occurs when the current <see cref="RenderService"/> instance/implementation is being disposed.
    /// </summary>
    protected override sealed void OnDispose(bool immediate)
    {
        _disposeRequested = true;
    }

    protected void DisposeBeforeRender()
    {
        base.OnDispose(true);

        _chain.Dispose();
        SpriteBatch.Dispose();

        foreach (GpuDevice device in _devices)
            device.Dispose();

        _devices.Clear();

        OnDisposeBeforeRender();

        Log.Dispose();
    }

    protected abstract void OnDisposeBeforeRender();

    /// <summary>
    /// Gets profiling data attached to the renderer.
    /// </summary>
    public GraphicsProfiler Profiler { get; private set; }

    /// <summary>
    /// Gets the display manager bound to the renderer.
    /// </summary>
    public GpuManager DisplayManager { get; private set; }

    /// <summary>
    /// Gets the primary <see cref="GpuDevice"/> bound to the current <see cref="RenderService"/>. 
    /// <para>This device is allocated the majority of render workload, but certain tasks may be offloaded to secondary devices, if available.</para>
    /// </summary>
    public GpuDevice Device { get; private set; }

    /// <summary>
    /// Gets a <see cref="GpuDevice"/> by it's index. The primary <see cref="Device"/> is always at index 0, if it exists.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    internal GpuDevice this[int index] => _devices[index];

    /// <summary>
    /// Gets a list of all the scenes current attached to the renderer.
    /// </summary>
    protected internal List<SceneRenderData> Scenes { get; } = new List<SceneRenderData>();

    /// <summary>
    /// Gets the width of the biggest render surface used so far.
    /// </summary>
    protected uint BiggestWidth { get; private set; } = 1;

    /// <summary>
    /// Gets the height of the biggest render surface used so far.
    /// </summary>
    protected uint BiggestHeight { get; private set; } = 1;

    /// <summary>
    /// Gets the renderer's <see cref="OverlayProvider"/> implementation.
    /// </summary>
    public OverlayProvider Overlay { get; }

    internal SurfaceManager Surfaces { get; }

    internal SpriteBatcher SpriteBatch { get; private set; }

    /// <summary>
    /// Gets the internal <see cref="SpriteFontManager"/> bound to the current <see cref="RenderService"/>.
    /// </summary>
    internal SpriteFontManager Fonts { get; private set; }

    /// <summary>
    /// Gets the incremental frame ID of the current <see cref="RenderService"/> instance. 
    /// This matches the value of render service thread <see cref="Timing.FrameID"/>.
    /// </summary>
    public ulong FrameID => Thread.Timing.FrameID;
}
