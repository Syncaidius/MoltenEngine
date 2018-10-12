using SharpDX.Direct3D11;
using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpDX.DXGI;

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

        ThreadedDictionary<string, SurfaceConfig> _surfacesByKey;
        ThreadedList<SurfaceConfig> _surfaces;
        ThreadedDictionary<MainSurfaceType, SurfaceConfig> _mainSurfaces;
        DepthSurface _depthSurface;

        internal SpriteBatcherDX11 SpriteBatcher;

        internal GraphicsBuffer StaticVertexBuffer;
        internal GraphicsBuffer DynamicVertexBuffer;
        internal StagingBuffer StagingBuffer;

        internal Material StandardMeshMaterial;
        internal Material StandardMeshMaterial_NoNormalMap;

        public RendererDX11()
        {
            _steps = new Dictionary<Type, RenderStepBase>();
            _stepList = new List<RenderStepBase>();
            _surfacesByKey = new ThreadedDictionary<string, SurfaceConfig>();
            _mainSurfaces = new ThreadedDictionary<MainSurfaceType, SurfaceConfig>();
            _surfaces = new ThreadedList<SurfaceConfig>();
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

            InitializeMainSurfaces(BiggestWidth, BiggestHeight);
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

        internal void InitializeMainSurfaces(int width, int height)
        {
            RegisterMainSurface("scene", MainSurfaceType.Scene, new RenderSurface(this, width, height, Format.R8G8B8A8_UNorm));
            RegisterMainSurface("normals", MainSurfaceType.Normals, new RenderSurface(this, width, height, Format.R11G11B10_Float));
            RegisterMainSurface("emissive", MainSurfaceType.Emissive, new RenderSurface(this, width, height, Format.R8G8B8A8_UNorm));
            RegisterMainSurface("composition1", MainSurfaceType.Composition1, new RenderSurface(this, width, height, Format.R16G16B16A16_Float));
            RegisterMainSurface("composition2", MainSurfaceType.Composition2, new RenderSurface(this, width, height, Format.R16G16B16A16_Float));
            RegisterMainSurface("lighting", MainSurfaceType.Lighting, new RenderSurface(this, width, height, Format.R16G16B16A16_Float));
            _depthSurface = new DepthSurface(this, width, height, DepthFormat.R24G8_Typeless);
        }

        internal SurfaceConfig RegisterSurface(string key, RenderSurfaceBase surface, SurfaceSizeMode sizeMode = SurfaceSizeMode.Full)
        {
            key = key.ToLower();
            if (!_surfacesByKey.TryGetValue(key, out SurfaceConfig config))
            {
                config = new SurfaceConfig(surface, sizeMode);
                _surfacesByKey.Add(key, config);
                _surfaces.Add(config);
            }

            return config;
        }

        internal void RegisterMainSurface(string key, MainSurfaceType mainType, RenderSurfaceBase surface, SurfaceSizeMode sizeMode = SurfaceSizeMode.Full)
        {
            SurfaceConfig config = RegisterSurface(key, surface, sizeMode);
            _mainSurfaces[mainType] = config;
        }

        internal T GetSurface<T>(MainSurfaceType type) where T: RenderSurfaceBase
        {
            return _mainSurfaces[type].Surface as T;
        }

        internal T GetSurface<T>(string key) where T : RenderSurfaceBase
        {
            return _surfacesByKey[key].Surface as T;
        }

        internal DepthSurface GetDepthSurface()
        {
            return _depthSurface;
        }

        internal T GetRenderStep<T>() where T : RenderStepBase, new()
        {
            Type t = typeof(T);
            RenderStepBase step;
            if (!_steps.TryGetValue(t, out step))
            {
                step = new T();
                step.Initialize(this);
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

        protected override void OnPreRenderScene(SceneRenderData sceneData, Timing time)
        {
            
        }

        protected override void OnPostRenderScene(SceneRenderData sceneData, Timing time)
        {
            // Clear the list of used surfaces, ready for the next frame.
            _clearedSurfaces.Clear();
        }

        protected override void OnPreRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            Device.Profiler = camera.Profiler;
        }

        protected override void OnPostRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            Device.Profiler = null;
        }

        protected override void OnPostPresent(Timing time)
        {

        }

        protected override void OnRebuildSurfaces(int requiredWidth, int requiredHeight)
        {
            _surfaces.ForInterlock(0, 1, (index, config) => config.RefreshSize(requiredWidth, requiredHeight));
            _depthSurface.Resize(requiredWidth, requiredHeight);
        }

        internal void RenderSceneLayer(GraphicsPipe pipe, LayerRenderData layerData, RenderCamera camera)
        {
            // To start with we're just going to draw ALL objects in the render tree.
            // Sorting and culling will come later
            LayerRenderData<Renderable> layer = layerData as LayerRenderData<Renderable>;

            layerData = layer as LayerRenderData<Renderable>;
            foreach (KeyValuePair<Renderable, List<ObjectRenderData>> p in layer.Renderables)
            {
                // TODO sort by material and textures
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

            _surfaces.ForInterlock(0, 1, (index, config) => config.Surface.Dispose());
            _surfaces.Clear();
            _depthSurface.Dispose();
            _mainSurfaces.Clear();
            _surfacesByKey.Clear();

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
