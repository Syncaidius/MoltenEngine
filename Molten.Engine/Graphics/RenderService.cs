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
        public static readonly Matrix4F DefaultView3D = Matrix4F.LookAtLH(new Vector3F(0, 0, -5), new Vector3F(0, 0, 0), Vector3F.UnitY);

        bool _disposeRequested;
        bool _shouldPresent;
        bool _surfaceResizeRequired;
        RenderChain _chain;
        AntiAliasLevel _requestedMultiSampleLevel = AntiAliasLevel.None;

        internal AntiAliasLevel MsaaLevel = AntiAliasLevel.None;
        internal Material StandardMeshMaterial;
        internal Material StandardMeshMaterial_NoNormalMap;

        internal IGraphicsBuffer StaticVertexBuffer;
        internal IGraphicsBuffer DynamicVertexBuffer;
        internal IStagingBuffer StagingBuffer;

        /// <summary>
        /// Creates a new instance of a <see cref="RenderService"/> sub-type.
        /// </summary>
        public RenderService()
        {
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
        /// Present's the renderer to it's bound output devices/surfaces.
        /// </summary>
        /// <param name="time"></param>
        protected override sealed void OnUpdate(Timing time)
        {
            /* DESIGN NOTES:
             *  - Store a hashset of materials used in each scene so that the renderer can set the "Common" buffer in one pass
             *  
             *  
             * MULTI-THREADING
             *  - Consider using 2+ worker threads to prepare a command list/deferred context from each scene, which can then
             *    be dispatched to the immediate context when all scenes have been processed
             *  - Avoid the above if any scenes interact with a render form surface at any point, since those can only be handled on the thread they're created on.
             *  
             *  - Consider using worker threads to:
             *      -- Sort front-to-back for rendering opaque objects (front-to-back reduces overdraw)
             *      -- Sort by buffer, material or textures (later in time)
             *      -- Sort back-to-front for rendering transparent objects (back-to-front reduces issues in alpha-blending)
             *  
             * 
             * 2D & UI Rendering:
             *  - Provide a sprite-batch for rendering 2D and UI
             *  - Prepare rendering of these on worker threads.
             */

            /* TODO: 
            *  Procedure:
            *      1) Calculate:
            *          a) Capture the current transform matrix of each object in the render tree
            *          b) Calculate the distance from the scene camera. Store on RenderData
            *          
            *      2) Sort objects by distance from camera:
            *          a) Sort objects into buckets inside RenderTree, front-to-back (reduce overdraw by drawing closest first).
            *          b) Only re-sort a bucket when objects are added or the camera moves
            *          c) While sorting, build up separate bucket list of objects with a transparent material sorted back-to-front (for alpha to work)
            *          
            *  Extras:
            *      3) Reduce z-sorting needed in (2) by adding scene-graph culling (quad-tree, octree or BSP) later down the line.
            *      4) Reduce (3) further by frustum culling the graph-culling results
            * 
            *  NOTES:
                - when SceneObject.IsVisible is changed, queue an Add or Remove operation on the RenderTree depending on visibility. This will remove it from culling/sorting.
            */

            if (_disposeRequested)
            {
                Surfaces.Dispose();
                DisposeBeforeRender();
                return;
            }

            if (!_shouldPresent)
                return;

            Profiler.Begin();
            Device.DisposeMarkedObjects();
            OnPrePresent(time);

            if (_requestedMultiSampleLevel != MsaaLevel)
            {
                // TODO re-create all internal surfaces/textures to match the new sample level.
                // TODO adjust rasterizer mode accordingly (multisample enabled/disabled).
                MsaaLevel = _requestedMultiSampleLevel;
                _surfaceResizeRequired = true;
            }

            // Perform all queued tasks before proceeding
            RendererTask task = null;
            while (Tasks.TryDequeue(out task))
                task.Process(this);

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

                Device.Cmd.BeginEvent("Draw Scene");
                sceneData.PreRenderInvoke(this);
                sceneData.Profiler.Begin();

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

                    Device.Cmd.Profiler = camera.Profiler;
                    camera.Profiler.Begin();
                    _chain.Render(sceneData, camera, time);
                    camera.Profiler.End(time);
                    Profiler.Accumulate(camera.Profiler.Previous);
                    sceneData.Profiler.Accumulate(camera.Profiler.Previous);
                    Device.Cmd.Profiler = null;
                }

                sceneData.Profiler.End(time);
                sceneData.PostRenderInvoke(this);
                Device.Cmd.EndEvent();
            }

            Surfaces.ResetFirstCleared();

            // Present all output surfaces
            OutputSurfaces.For(0, 1, (index, surface) =>
            {
                surface.Present();
                return false;
            });

            OnPostPresent(time);
            Profiler.End(time);
        }

        internal void RenderSceneLayer(GraphicsCommandQueue cmd, LayerRenderData layerData, RenderCamera camera)
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

        /// <summary>
        /// Occurs when the renderer is being initialized.
        /// </summary>
        /// <param name="settings"></param>
        protected override sealed void OnInitialize(EngineSettings settings)
        {
            DisplayManager = OnInitializeDisplayManager(settings.Graphics);
            _chain = new RenderChain(this);

            try
            {
                DisplayManager.Initialize(Log, settings.Graphics);
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
                Device = OnCreateDevice(settings.Graphics, DisplayManager);
                Device.Initialize();
                Log.WriteLine("Initialized graphics device");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to initialize graphics device");
                Log.Error(ex, true);
            }

            OnInitializeRenderer(settings);
            Compute = new ComputeManager(Device);

            uint maxBufferSize = (uint)ByteMath.FromMegabytes(3.5);
            StaticVertexBuffer = Device.CreateBuffer(GraphicsBufferFlags.Vertex | GraphicsBufferFlags.Index, BufferMode.Default, maxBufferSize);
            DynamicVertexBuffer = Device.CreateBuffer(GraphicsBufferFlags.Vertex | GraphicsBufferFlags.Index, BufferMode.DynamicRing, maxBufferSize);
            StagingBuffer = Device.CreateStagingBuffer(StagingBufferFlags.Write, maxBufferSize);
            SpriteBatch = new SpriteBatcher(this, 3000, 20);

            LoadDefaultShaders();

            Surfaces.Initialize(BiggestWidth, BiggestHeight);
            Fonts = new SpriteFontManager(Log, this);
            Fonts.Initialize();
        }

        private void LoadDefaultShaders()
        {
            ShaderCompileResult result = Resources.LoadEmbeddedShader("Molten.Assets", "gbuffer.mfx");
            StandardMeshMaterial = result[ShaderClassType.Material, "gbuffer"] as Material;
            StandardMeshMaterial_NoNormalMap = result[ShaderClassType.Material, "gbuffer-sans-nmap"] as Material;
        }

        protected abstract void OnInitializeRenderer(EngineSettings settings);

        private void MSAA_OnChanged(AntiAliasLevel oldValue, AntiAliasLevel newValue)
        {
            _requestedMultiSampleLevel = newValue;
        }

        internal SceneRenderData CreateRenderData()
        {
            SceneRenderData rd = new SceneRenderData();
            RendererAddScene task = RendererAddScene.Get();
            task.Data = rd;
            PushTask(task);
            return rd;
        }

        public void DestroyRenderData(SceneRenderData data)
        {
            RendererRemoveScene task = RendererRemoveScene.Get();
            task.Data = data;
            PushTask(task);
        }

        public void PushTask(RendererTask task)
        {
            Tasks.Enqueue(task);
        }

        /// <summary>
        /// Invoked during the first stage of service initialization to allow any api-related objects to be created/initialized prior to renderer initialization.
        /// </summary>
        /// <param name="settings">The <see cref="GraphicsSettings"/> bound to the current engine instance.</param>
        protected abstract GraphicsDisplayManager OnInitializeDisplayManager(GraphicsSettings settings);

        protected abstract GraphicsDevice OnCreateDevice(GraphicsSettings settings, GraphicsDisplayManager manager);

        /// <summary>
        /// Occurs before the render engine begins rendering all of the active scenes to the active output(s).
        /// </summary>
        /// <param name="time">A timing instance.</param>
        protected abstract void OnPrePresent(Timing time);

        /// <summary>
        /// Occurs after render presentation is completed and profiler timing has been finalized for the current frame. Useful if you need to do some per-frame cleanup/resetting.
        /// </summary>
        /// <param name="time">A timing instance.</param>
        protected abstract void OnPostPresent(Timing time);

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
            OutputSurfaces.For(0, 1, (index, surface) =>
            {
                surface.Dispose();
                return false;
            });

            _chain.Dispose();
            SpriteBatch.Dispose();
            StaticVertexBuffer.Dispose();
            DynamicVertexBuffer.Dispose();

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
        public GraphicsDisplayManager DisplayManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="GraphicsDevice"/> bound to the current <see cref="RenderService"/>.
        /// </summary>
        public GraphicsDevice Device { get; private set; }

        /// <summary>
        /// Gets the <see cref="ResourceFactory"/> bound to the renderer.
        /// This is responsible for creating and destroying graphics resources, such as buffers, textures and surfaces.
        /// </summary>
        public abstract ResourceFactory Resources { get; }

        /// <summary>
        /// Gets the compute manager attached to the current renderer.
        /// </summary>
        public ComputeManager Compute { get; private set; }

        /// <summary>
        /// Gets a list of all the output <see cref="ISwapChainSurface"/> instances attached to the renderer. These are automatically presented to the graphics device by the renderer, if active.
        /// </summary>
        public ThreadedList<ISwapChainSurface> OutputSurfaces { get; } = new ThreadedList<ISwapChainSurface>();

        /// <summary>
        /// Gets a list of all the scenes current attached to the renderer.
        /// </summary>
        protected internal List<SceneRenderData> Scenes { get; } = new List<SceneRenderData>();

        private ThreadedQueue<RendererTask> Tasks { get; } = new ThreadedQueue<RendererTask>();

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

        public SurfaceManager Surfaces { get; }

        public abstract ShaderCompiler Compiler { get; }

        internal SpriteBatcher SpriteBatch { get; private set; }

        /// <summary>
        /// Gets the internal <see cref="SpriteFontManager"/> bound to the current <see cref="RenderService"/>.
        /// </summary>
        internal SpriteFontManager Fonts { get; private set; }
    }
}
