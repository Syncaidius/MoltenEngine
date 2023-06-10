using System.Data;
using Molten.Collections;
using Molten.Graphics.Overlays;
using Molten.Threading;

namespace Molten.Graphics
{
    /// <summary>
    /// A base class that custom renderer implementations must inherit in order to be compatible with Molten engine, 
    /// as it provides basic functionality for interacting with the rest of the engine.
    /// </summary>
    public abstract class RenderService : EngineService
    {
        bool _disposeRequested;
        bool _shouldPresent;
        bool _surfaceResizeRequired;

        RenderFrameTracker _tracker;
        RenderChain _chain;

        Dictionary<RenderTaskPriority, ThreadedQueue<RenderTask>> _tasks;
        AntiAliasLevel _requestedMultiSampleLevel = AntiAliasLevel.None;

        internal AntiAliasLevel MsaaLevel = AntiAliasLevel.None;
        internal HlslShader FxStandardMesh;
        internal HlslShader FxStandardMesh_NoNormalMap;

        /// <summary>
        /// Creates a new instance of a <see cref="RenderService"/> sub-type.
        /// </summary>
        public RenderService()
        {
            _tasks = new Dictionary<RenderTaskPriority, ThreadedQueue<RenderTask>>();
            RenderTaskPriority[] priorities = Enum.GetValues<RenderTaskPriority>();
            foreach (RenderTaskPriority p in priorities)
                _tasks[p] = new ThreadedQueue<RenderTask>();

            Surfaces = new SurfaceManager(this);
            Overlay = new OverlayProvider();
            Log.WriteLine("Acquiring render chain");
        }

        protected override ThreadingMode OnStart(ThreadManager threadManager)
        {
            _shouldPresent = true;
            return ThreadingMode.SeparateThread;
        }

        protected override void OnStop()
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
                Device = OnInitializeDevice(settings.Graphics, DisplayManager);

                Log.WriteLine("Initialized graphics device");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to initialize graphics device");
                Log.Error(ex, true);
            }

            OnInitializeRenderer(settings);

            _tracker = new RenderFrameTracker(this, 5.5);
            SpriteBatch = new SpriteBatcher(this, 3000, 20);
            LoadDefaultShaders();

            Surfaces.Initialize(BiggestWidth, BiggestHeight);
            Fonts = new SpriteFontManager(Log, this);
            Fonts.Initialize();
        }

        private void ProcessTasks(RenderTaskPriority priority)
        {
            ThreadedQueue<RenderTask> queue = _tasks[priority];
            Device.Queue.BeginEvent($"Process '{priority}' tasks");
            while (queue.TryDequeue(out RenderTask task))
                task.Process(this);
            Device.Queue.EndEvent();
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

            _tracker.StartFrame();
            Profiler.Begin();
            Device.Queue.Profiler.Begin();
            Device.DisposeMarkedObjects();

            if (_requestedMultiSampleLevel != MsaaLevel)
            {
                // TODO re-create all internal surfaces/textures to match the new sample level.
                // TODO adjust rasterizer mode accordingly (multisample enabled/disabled).
                MsaaLevel = _requestedMultiSampleLevel;
                _surfaceResizeRequired = true;
            }

            ProcessTasks(RenderTaskPriority.StartOfFrame);

            // Perform preliminary checks on active scene data.
            // Also ensure the backbuffer is always big enough for the largest scene render surface.
            foreach (SceneRenderData data in Scenes)
            {
                data.ProcessChanges();

                foreach (RenderCamera camera in data.Cameras)
                {
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
            
            foreach (SceneRenderData sceneData in Scenes)
            {
                if (!sceneData.IsVisible)
                    continue;

                Device.Queue.BeginEvent("Draw Scene");
                sceneData.PreRenderInvoke(this);
                Device.Queue.PushProfiler(sceneData.Profiler);

                // Sort cameras into ascending order-depth.
                sceneData.Cameras.Sort((a, b) =>
                {
                    if (a.OrderDepth > b.OrderDepth)
                        return 1;
                    else if (a.OrderDepth < b.OrderDepth)
                        return -1;
                    else
                        return 0;
                });

                foreach (RenderCamera camera in sceneData.Cameras)
                {
                    if (camera.Skip)
                        continue;

                    Device.Queue.PushProfiler(camera.Profiler);
                    _chain.Render(sceneData, camera, time);
                    Device.Queue.PopProfiler(time);
                }

                Device.Queue.PopProfiler(time);
                sceneData.PostRenderInvoke(this);
                Device.Queue.EndEvent();
            }

            Surfaces.ResetFirstCleared();

            // Present all output surfaces
            OutputSurfaces.For(0, (index, surface) =>
            {
                surface.Present();
                return false;
            });

            ProcessTasks(RenderTaskPriority.EndOfFrame);

            Device.Queue.Profiler.End(time);
            Profiler.Accumulate(Device.Queue.Profiler.Previous, false);
            Profiler.End(time);
            _tracker.EndFrame();
        }

        internal void RenderSceneLayer(GraphicsQueue cmd, LayerRenderData layerData, RenderCamera camera)
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
            ShaderCompileResult result = Device.LoadEmbeddedShader("Molten.Assets", "gbuffer.mfx");
            FxStandardMesh = result["gbuffer"];
            FxStandardMesh_NoNormalMap = result["gbuffer-sans-nmap"];
        }

        private void MSAA_OnChanged(AntiAliasLevel oldValue, AntiAliasLevel newValue)
        {
            _requestedMultiSampleLevel = newValue;
        }

        internal SceneRenderData CreateRenderData()
        {
            SceneRenderData rd = new SceneRenderData();
            RenderAddScene task = RenderAddScene.Get();
            task.Data = rd;
            PushTask(RenderTaskPriority.StartOfFrame, task);
            return rd;
        }

        public void DestroyRenderData(SceneRenderData data)
        {
            RenderRemoveScene task = RenderRemoveScene.Get();
            task.Data = data;
            PushTask(RenderTaskPriority.StartOfFrame, task);
        }

        public void PushTask(RenderTaskPriority priority, RenderTask task)
        {
            _tasks[priority].Enqueue(task);
        }

        /// <summary>
        /// Pushes a compute-based shader as a task.
        /// </summary>
        /// <param name="shader">The compute shader to be run inside the task.</param>
        /// <param name="groupsX">The number of X compute thread groups.</param>
        /// <param name="groupsY">The number of Y compute thread groups.</param>
        /// <param name="groupsZ">The number of Z compute thread groups.</param>
        /// <param name="callback">A callback to run once the task is completed.</param>
        public void PushTask(RenderTaskPriority priority, HlslShader shader, uint groupsX, uint groupsY, uint groupsZ, ComputeTaskCompletionCallback callback = null)
        {
            PushComputeTask(priority, shader, new Vector3UI(groupsX, groupsY, groupsZ), callback);
        }

        public void PushComputeTask(RenderTaskPriority priority, HlslShader shader, Vector3UI groups, ComputeTaskCompletionCallback callback = null)
        {
            ComputeTask task = ComputeTask.Get();
            task.Shader = shader;
            task.Groups = groups;
            task.CompletionCallback = callback;
            PushTask(priority, task);
        }

        protected abstract void OnInitializeRenderer(EngineSettings settings);

        /// <summary>
        /// Invoked during the first stage of service initialization to allow any api-related objects to be created/initialized prior to renderer initialization.
        /// </summary>
        /// <param name="settings">The <see cref="GraphicsSettings"/> bound to the current engine instance.</param>
        protected abstract GraphicsManager OnInitializeDisplayManager(GraphicsSettings settings);

        protected abstract GraphicsDevice OnInitializeDevice(GraphicsSettings settings, GraphicsManager manager);

        /// <summary>
        /// Occurs when the current <see cref="RenderService"/> instance/implementation is being disposed.
        /// </summary>
        protected override sealed void OnDispose()
        {
            _disposeRequested = true;
        }

        protected void DisposeBeforeRender()
        {
            base.OnDispose();

            // Dispose of any registered output services.
            OutputSurfaces.For(0, (index, surface) =>
            {
                surface.Dispose();
                return false;
            });

            _chain.Dispose();
            SpriteBatch.Dispose();
            _tracker.Dispose();

            OnDisposeBeforeRender();

            Log.Dispose();
        }

        protected abstract void OnDisposeBeforeRender();

        /// <summary>
        /// Gets profiling data attached to the renderer.
        /// </summary>
        public RenderProfiler Profiler { get; } = new RenderProfiler();

        /// <summary>
        /// Gets the display manager bound to the renderer.
        /// </summary>
        public GraphicsManager DisplayManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="GraphicsDevice"/> bound to the current <see cref="RenderService"/>.
        /// </summary>
        public GraphicsDevice Device { get; private set; }

        /// <summary>
        /// Gets a list of all the output <see cref="ISwapChainSurface"/> instances attached to the renderer. These are automatically presented to the graphics device by the renderer, if active.
        /// </summary>
        public ThreadedList<ISwapChainSurface> OutputSurfaces { get; } = new ThreadedList<ISwapChainSurface>();

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

        /// <summary>
        /// Gets the <see cref="ShaderCompiler"/> that is bound to the current <see cref="RenderService"/>.
        /// </summary>
        protected internal abstract ShaderCompiler Compiler { get; }

        internal SpriteBatcher SpriteBatch { get; private set; }

        /// <summary>
        /// Gets the internal <see cref="SpriteFontManager"/> bound to the current <see cref="RenderService"/>.
        /// </summary>
        internal SpriteFontManager Fonts { get; private set; }

        /// <summary>
        /// Gets the tracked resources for the current frame.
        /// </summary>
        public RenderFrameTracker.TrackedFrame Frame => _tracker.Frame;

        /// <summary>
        /// Gets the current frame index. The value will be between 0 and <see cref="GraphicsSettings.BufferingMode"/> - 1, from <see cref="GraphicsDevice.Settings"/>.
        /// </summary>
        public uint BackBufferIndex => _tracker.BackBufferIndex;
    }
}
