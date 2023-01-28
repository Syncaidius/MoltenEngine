using Molten.Graphics.Dxgi;
using Silk.NET.Direct3D12;

namespace Molten.Graphics
{
    public class RendererDX12 : RenderService
    {
        D3D12 _api;
        DisplayManagerDXGI _displayManager;

        public RendererDX12()
        {
            
        }

        protected override GraphicsDisplayManager OnInitializeDisplayManager(GraphicsSettings settings)
        {
            _api = D3D12.GetApi();
            Builder = new DeviceBuilderDX12(_api, this);
            _displayManager = new DisplayManagerDXGI(Builder.GetCapabilities);
            return _displayManager;
        }


        protected override GraphicsDevice OnCreateDevice(GraphicsSettings settings, GraphicsDisplayManager manager)
        {
            NativeDevice = new DeviceDX12(settings, Builder, Log, _displayManager.SelectedAdapter);
            return NativeDevice;
        }

        protected override void OnInitializeRenderer(EngineSettings settings)
        {
            
        }

        protected override SceneRenderData OnCreateRenderData()
        {
            throw new NotImplementedException();
        }

        protected override void OnPostPresent(Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPostRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPostRenderScene(SceneRenderData sceneData, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPrePresent(Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPreRenderCamera(SceneRenderData sceneData, RenderCamera camera, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPreRenderScene(SceneRenderData sceneData, Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnDisposeBeforeRender()
        {
            NativeDevice?.Dispose();
            _api.Dispose();
        }

        public override void BuildRenderChain(RenderChainLink first, SceneRenderData scene, LayerRenderData layerData, RenderCamera camera)
        {
            throw new NotImplementedException();
        }

        internal DeviceDX12 NativeDevice { get; private set; }

        internal DeviceBuilderDX12 Builder { get; private set; }

        public override ResourceFactory Resources { get; }

        public override DxcCompiler Compiler { get; }

        public override SpriteBatcher SpriteBatch => throw new NotImplementedException();
    }
}
