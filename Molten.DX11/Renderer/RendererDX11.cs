using SharpDX.Direct3D11;
using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Molten.Graphics
{
    public class RendererDX11 : RenderEngine
    {
        int _biggestWidth = 1;
        int _biggestHeight = 1;
        bool _surfaceResizeRequired;

        DisplayManagerDX11 _displayManager;
        ResourceManager _resourceManager;
        ComputeManager _compute;
        GraphicsDeviceDX11 _device;
        Logger _log;

        HlslCompiler _shaderCompiler;
        ThreadedList<ISwapChainSurface> _outputSurfaces;
        HashSet<TextureAsset2D> _clearedSurfaces;
        Dictionary<Type, RenderStepBase> _steps;
        List<RenderStepBase> _stepList;
        RenderChain _chain;

        AntiAliasMode _requestedMultiSampleLevel = AntiAliasMode.None;
        internal AntiAliasMode MsaaLevel = AntiAliasMode.None;
        internal SpriteBatchDX11 SpriteBatcher;

        internal GraphicsBuffer StaticVertexBuffer;
        internal GraphicsBuffer DynamicVertexBuffer;
        internal StagingBuffer StagingBuffer;

        internal Material StandardMeshMaterial;
        internal Material StandardMeshMaterial_NoNormalMap;

        internal List<DebugOverlayPage> DebugOverlayPages;

        public RendererDX11()
        {
            _log = Logger.Get();
            _log.AddOutput(new LogFileWriter("renderer_dx11{0}.txt"));
            _steps = new Dictionary<Type, RenderStepBase>();
            _stepList = new List<RenderStepBase>();
            _chain = new RenderChain(this);

            DebugOverlayPages = new List<DebugOverlayPage>();
            DebugOverlayPages.Add(new DebugStatsPage());
            DebugOverlayPages.Add(new DebugBuffersPage());
            DebugOverlayPages.Add(new DebugFeaturesPage());
            DebugOverlayPages.Add(new RawSceneOverlay());
            DebugOverlayPages.Add(new DepthOverlay());
            DebugOverlayPages.Add(new NormalsOverlay());
            DebugOverlayPages.Add(new EmissiveOverlay());
            DebugOverlayPages.Add(new LightingOverlay());
        }

        public override void InitializeAdapter(GraphicsSettings settings)
        {
            _displayManager = new DisplayManagerDX11();
            _displayManager.Initialize(_log, settings);
        }

        public override void Initialize(GraphicsSettings settings)
        {
            settings.Log(_log, "Graphics");
            MsaaLevel = _requestedMultiSampleLevel = MsaaLevel;
            settings.MSAA.OnChanged += MSAA_OnChanged;

            _outputSurfaces = new ThreadedList<ISwapChainSurface>();
            _device = new GraphicsDeviceDX11(_log, settings, Profiler, _displayManager, settings.EnableDebugLayer);
            _resourceManager = new ResourceManager(this);
            _compute = new ComputeManager(this.Device);
            _shaderCompiler = new HlslCompiler(this, _log);
            _clearedSurfaces = new HashSet<TextureAsset2D>();

            int maxBufferSize = (int)ByteMath.FromMegabytes(3.5);
            StaticVertexBuffer = new GraphicsBuffer(_device, BufferMode.Default, BindFlags.VertexBuffer | BindFlags.IndexBuffer, maxBufferSize);
            DynamicVertexBuffer = new GraphicsBuffer(_device, BufferMode.DynamicRing, BindFlags.VertexBuffer | BindFlags.IndexBuffer, maxBufferSize);

            StagingBuffer = new StagingBuffer(_device, StagingBufferFlags.Write, maxBufferSize);
            SpriteBatcher = new SpriteBatchDX11(this, 3000);

            LoadDefaultShaders();
        }

        private void LoadDefaultShaders()
        {
            string source = null;
            string namepace = "Molten.Graphics.Assets.gbuffer.sbm";
            using (Stream stream = EmbeddedResource.GetStream(namepace, typeof(RendererDX11).Assembly))
            {
                using (StreamReader reader = new StreamReader(stream))
                    source = reader.ReadToEnd();
            }

            if (!string.IsNullOrWhiteSpace(source))
            {
                ShaderCompileResult result = _shaderCompiler.Compile(source, namepace);
                StandardMeshMaterial = result["material", "gbuffer"] as Material;
                StandardMeshMaterial_NoNormalMap = result["material", "gbuffer-sans-nmap"] as Material;
            }
        }

        public void DispatchCompute(IComputeTask task, int x, int y, int z)
        {
            _device.Dispatch(task as ComputeTask, x, y, z);
        }

        internal T GetRenderStep<T>() where T : RenderStepBase, new()
        {
            Type t = typeof(T);
            RenderStepBase step;
            if (!_steps.TryGetValue(t, out step))
            {
                step = new T();
                step.Initialize(this, _biggestWidth, _biggestHeight);
                _steps.Add(t, step);
                _stepList.Add(step);
            }

            return step as T;
        }

        protected override SceneRenderData OnCreateRenderData()
        {
            SceneRenderData data =  new SceneRenderData<Renderable>();
            data.DebugOverlay = new SceneDebugOverlay(this, data);
            return data;
        }

        protected override void OnPresent(Timing time)
        {            
            _device.DisposeMarkedObjects();

            if(_requestedMultiSampleLevel != MsaaLevel)
            {
                // TODO re-create all internal surfaces/textures to match the new sample level.
                // TODO adjust rasterizer mode accordingly (multisample enabled/disabled).
                MsaaLevel = _requestedMultiSampleLevel;
                _surfaceResizeRequired = true;
            }

            ProcessPendingTasks();

            // Perform preliminary checks on active scene data.
            // Also ensure the backbuffer is always big enough for the largest scene render surface.
            foreach (SceneRenderData data in Scenes)
            {
                data.Skip = false;

                if (!data.IsVisible || data.Camera == null)
                {
                    data.Skip = true;
                    continue;
                }

                // Check for valid final surface.
                data.FinalSurface = data.Camera.OutputSurface ?? _device.DefaultSurface;
                if (data.FinalSurface == null)
                {
                    data.Skip = true;
                    continue;
                }

                if (data.FinalSurface.Width > _biggestWidth)
                {
                    _surfaceResizeRequired = true;
                    _biggestWidth = data.FinalSurface.Width;
                }

                if (data.FinalSurface.Height > _biggestHeight)
                {
                    _surfaceResizeRequired = true;
                    _biggestHeight = data.FinalSurface.Height;
                }
            }

            // Update surfaces if dirty. This may involve resizing or changing their format.
            if (_surfaceResizeRequired)
            {
                for (int i = 0; i < _stepList.Count; i++)
                    _stepList[i].UpdateSurfaces(this, _biggestWidth, _biggestHeight);

                _surfaceResizeRequired = false;
            }

            /* CAMERA REFACTOR
             *  - The renderer will iterate over cameras instead of scenes
             *  - Each camera will store a 32-bit layer mask (32 layers, one per bit)
             *  - RenderData will be split into LayerData objects, containing the objects to be rendered on each layer
             *  - The layer limit will be 32
             *  - A camera will be given the RenderData object of the scene it is part of.
             */

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

            SceneRenderData<Renderable> scene;
            for (int i = 0; i < Scenes.Count; i++)
            {
                scene = Scenes[i] as SceneRenderData<Renderable>;
                if (scene.Skip)
                    continue;

                _device.Profiler = scene.Profiler;
                _device.Profiler.StartCapture();
                RenderScene(scene, _device, time);

                Profiler.AddData(scene.Profiler.CurrentFrame);
                _device.Profiler.EndCapture(time);
                _device.Profiler = null;
            }

            // Present all output surfaces
            _outputSurfaces.ForInterlock(0, 1, (index, surface) =>
            {                
                surface.Present();
                return false;
            });

            // Clear references to final surfaces. 
            // This is done separately so that any debug overlays rendered by scenes can still access final surface information during their render call.
            for(int i = 0; i < Scenes.Count; i++)
            {
                scene = Scenes[i] as SceneRenderData<Renderable>;
                scene.FinalSurface = null;
            }

            // Clear the list of used surfaces, ready for the next frame.
            _clearedSurfaces.Clear();
            Profiler.AddData(_device.Profiler.CurrentFrame);
        }

        internal void RenderScene(SceneRenderData<Renderable> data, GraphicsPipe pipe, Timing time)
        {
            data.PreRenderInvoke(this);
            data.ProcessChanges();
            _chain.Build(data);
            _chain.Render(data, time);
            data.PostRenderInvoke(this);
        }

        internal void Render3D(GraphicsPipe pipe, SceneRenderData<Renderable> sceneData)
        {
            // To start with we're just going to draw ALL objects in the render tree.
            // Sorting and culling will come later
            foreach (KeyValuePair<Renderable, List<ObjectRenderData>> p in sceneData.Renderables)
            {
                foreach (ObjectRenderData data in p.Value)
                {
                    // TODO replace below with render prediction to interpolate between the current and target transform.
                    data.RenderTransform = data.TargetTransform;
                    p.Key.Render(pipe, this, data, sceneData);
                }
            }

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
        }

        internal void Render2D(GraphicsPipe pipe, SceneRenderData sceneData)
        {
            for (int i = 0; i < sceneData.Renderables2D.Count; i++)
                sceneData.Renderables2D[i].Render(SpriteBatcher);
        }

        internal bool ClearIfFirstUse(TextureAsset2D surface, Action callback)
        {
            if(!_clearedSurfaces.Contains(surface))
            {
                callback();
                _clearedSurfaces.Add(surface);
                return true;
            }

            return false;
        }

        private void MSAA_OnChanged(AntiAliasMode oldValue, AntiAliasMode newValue)
        {
            _requestedMultiSampleLevel = newValue;
        }

        public override void Dispose()
        {
            for (int i = 0; i < _stepList.Count; i++)
                _stepList[i].Dispose();

            _outputSurfaces.ForInterlock(0, 1, (index, surface) =>
            {
                surface.Dispose();
                return false;
            });

            _resourceManager.Dispose();
            _displayManager?.Dispose();
            SpriteBatcher.Dispose();

            StaticVertexBuffer.Dispose();
            DynamicVertexBuffer.Dispose();
            _device?.Dispose();
            _log.Dispose();
        }

        /// <summary>
        /// Gets the name of the renderer.
        /// </summary>
        public override string Name => "DirectX 11";

        /// <summary>
        /// Gets the display manager bound to the renderer.
        /// </summary>
        public override IDisplayManager DisplayManager => _displayManager;

        internal GraphicsDeviceDX11 Device => _device;

        public override IComputeManager Compute => _compute;

        internal HlslCompiler ShaderCompiler => _shaderCompiler;

        /// <summary>
        /// Gets the resource manager bound to the renderer.
        /// This is responsible for creating and destroying graphics resources, such as buffers, textures and surfaces.
        /// </summary>
        public override IResourceManager Resources => _resourceManager;

        public override ThreadedList<ISwapChainSurface> OutputSurfaces => _outputSurfaces;

        public override IRenderSurface DefaultSurface
        {
            get => _device.DefaultSurface;
            set => _device.DefaultSurface = value as RenderSurfaceBase;
        }
    }
}
