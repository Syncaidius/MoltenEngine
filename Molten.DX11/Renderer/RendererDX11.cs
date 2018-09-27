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
    public class RendererDX11 : MoltenRenderer
    {
        DisplayManagerDX11 _displayManager;
        ResourceManager _resourceManager;
        ComputeManager _compute;
        HashSet<TextureAsset2D> _clearedSurfaces;
        Dictionary<Type, RenderStepBase> _steps;
        List<RenderStepBase> _stepList;

        internal SpriteBatcherDX11 SpriteBatcher;

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
            SpriteBatcher = new SpriteBatcherDX11(this, 3000);

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
            return new SceneRenderData<Renderable>();
        }

        protected override IRenderChain GetRenderChain()
        {
            return new RenderChain(this);
        }

        protected override void OnPrePresent(Timing time)
        {
            Device.DisposeMarkedObjects();
        }

        protected override void OnPreRenderScene(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            Device.Profiler = camera.Profiler;
            Device.Profiler.Begin();
        }

        protected override void OnPostRenderScene(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            Device.Profiler.End(time);
            Device.Profiler = null;
        }

        protected override void OnPostPresent(Timing time)
        {
            // Clear the list of used surfaces, ready for the next frame.
            _clearedSurfaces.Clear();
        }

        protected override void OnRebuildSurfaces(int requiredWidth, int requiredHeight)
        {
            for (int i = 0; i < _stepList.Count; i++)
                _stepList[i].UpdateSurfaces(this, requiredWidth, requiredHeight);
        }

        internal void Render3D(GraphicsPipe pipe, LayerRenderData layerData, RenderCamera camera)
        {
            // To start with we're just going to draw ALL objects in the render tree.
            // Sorting and culling will come later
            LayerRenderData<Renderable> layer = layerData as LayerRenderData<Renderable>;

            layerData = layer as LayerRenderData<Renderable>;
            foreach (KeyValuePair<Renderable, List<ObjectRenderData>> p in layer.Renderables)
            {
                // TODO use instancing here.
                foreach (ObjectRenderData data in p.Value)
                {
                    // TODO replace below with render prediction to interpolate between the current and target transform.
                    data.RenderTransform = data.TargetTransform;
                    p.Key.Render(pipe, this, camera, data);
                }
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
    }
}
