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

            _spriteBatcher = new SpriteBatcherDX11(this, 3000, 20);
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

        protected override void OnPostPresent(Timing time) { }

        protected override void OnDisposeBeforeRender()
        {
            _resFactory.Dispose();
            _displayManager.Dispose();

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
