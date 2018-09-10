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
        DisplayManagerDX11 _displayManager;
        ResourceManager _resourceManager;
        ComputeManager _compute;
        HashSet<TextureAsset2D> _clearedSurfaces;
        Dictionary<Type, RenderStepBase> _steps;
        List<RenderStepBase> _stepList;
        RenderChain _chain;

        internal SpriteBatchDX11 SpriteBatcher;

        internal GraphicsBuffer StaticVertexBuffer;
        internal GraphicsBuffer DynamicVertexBuffer;
        internal StagingBuffer StagingBuffer;

        internal Material StandardMeshMaterial;
        internal Material StandardMeshMaterial_NoNormalMap;

        internal List<DebugOverlayPage> DebugOverlayPages;

        public RendererDX11()
        {
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

        protected override void OnInitializeAdapter(GraphicsSettings settings)
        {
            _displayManager = new DisplayManagerDX11();
            _displayManager.Initialize(Log, settings);
        }

        protected override void OnInitialize(GraphicsSettings settings)
        {
            Device = new GraphicsDeviceDX11(Log, settings, Profiler, _displayManager, settings.EnableDebugLayer);
            _resourceManager = new ResourceManager(this);
            _compute = new ComputeManager(this.Device);
            ShaderCompiler = new HlslCompiler(this, Log);
            _clearedSurfaces = new HashSet<TextureAsset2D>();

            int maxBufferSize = (int)ByteMath.FromMegabytes(3.5);
            StaticVertexBuffer = new GraphicsBuffer(Device, BufferMode.Default, BindFlags.VertexBuffer | BindFlags.IndexBuffer, maxBufferSize);
            DynamicVertexBuffer = new GraphicsBuffer(Device, BufferMode.DynamicRing, BindFlags.VertexBuffer | BindFlags.IndexBuffer, maxBufferSize);

            StagingBuffer = new StagingBuffer(Device, StagingBufferFlags.Write, maxBufferSize);
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
                ShaderCompileResult result = ShaderCompiler.Compile(source, namepace);
                StandardMeshMaterial = result["material", "gbuffer"] as Material;
                StandardMeshMaterial_NoNormalMap = result["material", "gbuffer-sans-nmap"] as Material;
            }
        }

        public void DispatchCompute(IComputeTask task, int x, int y, int z)
        {
            Device.Dispatch(task as ComputeTask, x, y, z);
        }

        internal T GetRenderStep<T>() where T : RenderStepBase, new()
        {
            Type t = typeof(T);
            RenderStepBase step;
            if (!_steps.TryGetValue(t, out step))
            {
                step = new T();
                step.Initialize(this, BiggestWidth, BiggestHeight);
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

        protected override void OnPrePresent(Timing time)
        {
            Device.DisposeMarkedObjects();
        }

        protected override void OnPresent(Timing time)
        {          
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
            foreach(SceneRenderData data in Scenes)
            {
                scene = data as SceneRenderData<Renderable>;

                Device.Profiler = scene.Profiler;
                Device.Profiler.StartCapture();
                data.PreRenderInvoke(this);
                foreach (RenderCamera camera in scene.Cameras)
                {
                    if (camera.Skip)
                        continue;                    

                    RenderScene(scene, camera, Device, time);                    
                    Device.Profiler = null;
                }
                data.PostRenderInvoke(this);
                Profiler.AddData(scene.Profiler.CurrentFrame);
                Device.Profiler.EndCapture(time);
            }
        }

        protected override void OnPostPresent(Timing time)
        {
            // Clear the list of used surfaces, ready for the next frame.
            _clearedSurfaces.Clear();
            Profiler.AddData(Device.Profiler.CurrentFrame);
        }

        protected override void OnRebuildSurfaces(int requiredWidth, int requiredHeight)
        {
            for (int i = 0; i < _stepList.Count; i++)
                _stepList[i].UpdateSurfaces(this, requiredWidth, requiredHeight);
        }

        internal void RenderScene(SceneRenderData<Renderable> data, RenderCamera camera, GraphicsPipe pipe, Timing time)
        {
            _chain.Build(data, camera);
            _chain.Render(data, camera, time);
        }

        internal void Render3D(GraphicsPipe pipe, SceneRenderData<Renderable> sceneData)
        {
            // To start with we're just going to draw ALL objects in the render tree.
            // Sorting and culling will come later
            SceneLayerData<Renderable> layerData;
            foreach (SceneLayerData layer in sceneData.Layers)
            {
                layerData = layer as SceneLayerData<Renderable>;
                foreach (KeyValuePair<Renderable, List<ObjectRenderData>> p in layerData.Renderables)
                {
                    // TODO use instancing here.
                    foreach (ObjectRenderData data in p.Value)
                    {
                        // TODO replace below with render prediction to interpolate between the current and target transform.
                        data.RenderTransform = data.TargetTransform;
                        p.Key.Render(pipe, this, data, sceneData);
                    }
                }
            }
        }

        internal void Render2D(GraphicsPipe pipe, SceneRenderData sceneData)
        {
            foreach (SceneLayerData layer in sceneData.Layers)
            {
                for (int i = 0; i < layer.Renderables2D.Count; i++)
                    layer.Renderables2D[i].Render(SpriteBatcher);
            }
        }

        internal bool ClearIfFirstUse(GraphicsPipe pipe, RenderSurfaceBase surface, Color color)
        {
            if (!_clearedSurfaces.Contains(surface))
            {
                surface.Clear(pipe, color);
                _clearedSurfaces.Add(surface);
                return true;
            }

            return false;
        }

        internal bool ClearIfFirstUse(GraphicsPipe pipe, DepthSurface surface, DepthStencilClearFlags flags = DepthStencilClearFlags.Depth, float depth = 1.0f, byte stencil = 0)
        {
            if (!_clearedSurfaces.Contains(surface))
            {
                surface.Clear(pipe, flags, depth, stencil);
                _clearedSurfaces.Add(surface);
                return true;
            }

            return false;
        }

        protected override void OnDispose()
        {
            for (int i = 0; i < _stepList.Count; i++)
                _stepList[i].Dispose();

            _resourceManager.Dispose();
            _displayManager?.Dispose();
            SpriteBatcher.Dispose();

            StaticVertexBuffer.Dispose();
            DynamicVertexBuffer.Dispose();
            Device?.Dispose();            
        }

        /// <summary>
        /// Gets the name of the renderer.
        /// </summary>
        public override string Name => "DirectX 11";

        /// <summary>
        /// Gets the display manager bound to the renderer.
        /// </summary>
        public override IDisplayManager DisplayManager => _displayManager;

        internal GraphicsDeviceDX11 Device { get; private set; }

        public override IComputeManager Compute => _compute;

        internal HlslCompiler ShaderCompiler { get; private set; }

        /// <summary>
        /// Gets the resource manager bound to the renderer.
        /// This is responsible for creating and destroying graphics resources, such as buffers, textures and surfaces.
        /// </summary>
        public override IResourceManager Resources => _resourceManager;

        public override IRenderSurface DefaultSurface
        {
            get => Device.DefaultSurface;
            set => Device.DefaultSurface = value as RenderSurfaceBase;
        }
    }
}
