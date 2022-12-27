using Molten.Collections;
using Molten.Graphics.Dxgi;
using Silk.NET.Direct3D11;
using System.Reflection;

namespace Molten.Graphics
{
    public class RendererDX11 : RenderService
    {
        D3D11 _api;
        RenderChain _chain;
        DisplayManagerDXGI _displayManager;
        ResourceFactoryDX11 _resFactory;
        ComputeManager _compute;
        HashSet<Texture2D> _clearedSurfaces;
        Dictionary<Type, RenderStepBase> _steps;
        List<RenderStepBase> _stepList;
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
            _displayManager = new DisplayManagerDXGI();

            Surfaces = new SurfaceManager(this);
        }

        protected override void OnInitializeApi(GraphicsSettings settings)
        {
            _api = D3D11.GetApi();
            _chain = new RenderChain(this);
        }

        protected override void OnInitialize(EngineSettings settings)
        {
            base.OnInitialize(settings);

            Assembly includeAssembly = this.GetType().Assembly;

            Device = new DeviceDX11(_api, Log, settings.Graphics, _displayManager);
            ShaderCompiler = new FxcCompiler(this, Log, "\\Assets\\HLSL\\include\\", includeAssembly);
            _resFactory = new ResourceFactoryDX11(this);
            _compute = new ComputeManager(this.Device);
            _clearedSurfaces = new HashSet<Texture2D>();

            uint maxBufferSize = (uint)ByteMath.FromMegabytes(3.5);
            StaticVertexBuffer = new GraphicsBuffer(Device, BufferMode.Default, BindFlag.VertexBuffer | BindFlag.IndexBuffer, maxBufferSize);
            DynamicVertexBuffer = new GraphicsBuffer(Device, BufferMode.DynamicRing, BindFlag.VertexBuffer | BindFlag.IndexBuffer, maxBufferSize);

            StagingBuffer = new StagingBuffer(Device, StagingBufferFlags.Write, maxBufferSize);
            SpriteBatcher = new SpriteBatcherDX11(this, 3000, 20);

            Surfaces.Initialize(BiggestWidth, BiggestHeight);
            LoadDefaultShaders(includeAssembly);
        }

        private void LoadDefaultShaders(Assembly includeAssembly)
        {
            ShaderCompileResult result = Resources.LoadEmbeddedShader("Molten.Graphics.Assets", "gbuffer.mfx", includeAssembly);
            StandardMeshMaterial = result[ShaderClassType.Material, "gbuffer"] as Material;
            StandardMeshMaterial_NoNormalMap = result[ShaderClassType.Material, "gbuffer-sans-nmap"] as Material;
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

        protected override void OnRebuildSurfaces(uint requiredWidth, uint requiredHeight)
        {
            Surfaces.Rebuild(requiredWidth, requiredHeight);
        }

        internal void RenderSceneLayer(DeviceContext pipe, LayerRenderData layerData, RenderCamera camera)
        {
            // TODO To start with we're just going to draw ALL objects in the render tree.
            // Sorting and culling will come later
            LayerRenderData<Renderable> layer = layerData as LayerRenderData<Renderable>;

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

        internal bool ClearIfFirstUse(DeviceContext context, RenderSurface2D surface, Color color)
        {
            if (!_clearedSurfaces.Contains(surface))
            {
                surface.Clear(context, color);
                _clearedSurfaces.Add(surface);
                return true;
            }

            return false;
        }

        internal bool ClearIfFirstUse(DeviceContext context, DepthStencilSurface surface,
            ClearFlag flags = ClearFlag.Depth, 
            float depth = 1.0f, byte stencil = 0)
        {
            if (!_clearedSurfaces.Contains(surface))
            {
                surface.Clear(context, flags, depth, stencil);
                _clearedSurfaces.Add(surface);
                return true;
            }

            return false;
        }

        protected override void OnDisposeBeforeRender()
        {
            for (int i = 0; i < _stepList.Count; i++)
                _stepList[i].Dispose();

            Surfaces.Dispose();

            _resFactory.Dispose();
            _displayManager.Dispose();
            SpriteBatcher.Dispose();

            StaticVertexBuffer.Dispose();
            DynamicVertexBuffer.Dispose();
            Device?.Dispose();
            _api.Dispose();
        }

        /// <summary>
        /// Gets the display manager bound to the renderer.
        /// </summary>
        public override IDisplayManager DisplayManager => _displayManager;

        internal DeviceDX11 Device { get; private set; }

        public override IComputeManager Compute => _compute;

        protected override IRenderChain Chain => _chain;

        internal FxcCompiler ShaderCompiler { get; private set; }

        /// <summary>
        /// Gets the resource manager bound to the renderer.
        /// This is responsible for creating and destroying graphics resources, such as buffers, textures and surfaces.
        /// </summary>
        public override ResourceFactory Resources => _resFactory;

        internal SurfaceManager Surfaces { get; }
    }
}
