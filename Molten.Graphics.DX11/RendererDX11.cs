using System.Reflection;
using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    public class RendererDX11 : RenderService
    {
        D3D11 _api;
        DisplayManagerDXGI _displayManager;
        DeviceBuilderDX11 _deviceBuilder;
        ResourceFactoryDX11 _resFactory;
        FxcCompiler _shaderCompiler;
        SpriteBatcherDX11 _spriteBatcher;

        internal GraphicsBuffer StaticVertexBuffer;
        internal GraphicsBuffer DynamicVertexBuffer;
        internal StagingBuffer StagingBuffer;

        internal Material StandardMeshMaterial;
        internal Material StandardMeshMaterial_NoNormalMap;

        protected unsafe override GraphicsDisplayManager OnInitializeDisplayManager(GraphicsSettings settings)
        {
            _api = D3D11.GetApi();
            _deviceBuilder = new DeviceBuilderDX11(_api, this, 
                D3DFeatureLevel.Level111,
                D3DFeatureLevel.Level110, 
                D3DFeatureLevel.Level101, 
                D3DFeatureLevel.Level100);
            _displayManager = new DisplayManagerDXGI(_deviceBuilder.GetCapabilities);

            return _displayManager;
        }

        protected override GraphicsDevice OnCreateDevice(GraphicsSettings settings, GraphicsDisplayManager manager)
        {
            NativeDevice = new DeviceDX11(settings, _deviceBuilder, Log, _displayManager.SelectedAdapter);
            return NativeDevice;
        }

        protected override void OnInitializeRenderer(EngineSettings settings)
        {
            Assembly includeAssembly = GetType().Assembly;
            
            _shaderCompiler = new FxcCompiler(this, Log, "\\Assets\\HLSL\\include\\", includeAssembly);
            _resFactory = new ResourceFactoryDX11(this);

            uint maxBufferSize = (uint)ByteMath.FromMegabytes(3.5);
            StaticVertexBuffer = new GraphicsBuffer(NativeDevice, BufferMode.Default, BindFlag.VertexBuffer | BindFlag.IndexBuffer, maxBufferSize);
            DynamicVertexBuffer = new GraphicsBuffer(NativeDevice, BufferMode.DynamicRing, BindFlag.VertexBuffer | BindFlag.IndexBuffer, maxBufferSize);

            StagingBuffer = new StagingBuffer(NativeDevice, StagingBufferFlags.Write, maxBufferSize);
            _spriteBatcher = new SpriteBatcherDX11(this, 3000, 20);

            LoadDefaultShaders(includeAssembly);
        }

        private void LoadDefaultShaders(Assembly includeAssembly)
        {
            ShaderCompileResult result = Resources.LoadEmbeddedShader("Molten.Graphics.Assets", "gbuffer.mfx", includeAssembly);
            StandardMeshMaterial = result[ShaderClassType.Material, "gbuffer"] as Material;
            StandardMeshMaterial_NoNormalMap = result[ShaderClassType.Material, "gbuffer-sans-nmap"] as Material;
        }

        protected override SceneRenderData OnCreateRenderData()
        {
            return new SceneRenderData<Renderable>();
        }

        protected override void OnPrePresent(Timing time) { }

        protected override void OnPreRenderScene(SceneRenderData sceneData, Timing time) { }

        protected override void OnPostRenderScene(SceneRenderData sceneData, Timing time) { }

        protected override void OnPreRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            NativeDevice.Cmd.Profiler = camera.Profiler;
        }

        protected override void OnPostRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            NativeDevice.Cmd.Profiler = null;
        }

        protected override void OnPostPresent(Timing time)
        {

        }

        internal void RenderSceneLayer(GraphicsCommandQueue cmd, LayerRenderData layerData, RenderCamera camera)
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
                    p.Key.Render(cmd, this, camera, data);
                }
            }
        }

        public override void BuildRenderChain(RenderChainLink first, SceneRenderData scene, LayerRenderData layerData, RenderCamera camera)
        {
            if (camera.Flags.HasFlag(RenderCameraFlags.Deferred))
                first.Next<GBufferStep>().Next<LightingStep>().Next<CompositionStep>().Next<SkyboxStep>().Next<FinalizeStep>();
            else
                first.Next<ForwardStep>().Next<SkyboxStep>().Next<FinalizeStep>();
        }

        protected override void OnDisposeBeforeRender()
        {
            _resFactory.Dispose();
            _displayManager.Dispose();
            SpriteBatch.Dispose();

            StaticVertexBuffer.Dispose();
            DynamicVertexBuffer.Dispose();
            NativeDevice?.Dispose();
            _api.Dispose();
        }

        internal DeviceDX11 NativeDevice { get; private set; }

        public override FxcCompiler Compiler => _shaderCompiler;

        /// <summary>
        /// Gets the resource manager bound to the renderer.
        /// This is responsible for creating and destroying graphics resources, such as buffers, textures and surfaces.
        /// </summary>
        public override ResourceFactory Resources => _resFactory;

        public override SpriteBatcherDX11 SpriteBatch => _spriteBatcher;
    }
}
