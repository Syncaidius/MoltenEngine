using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics
{
    public class RendererDX12 : RenderService
    {
        D3D12 _api;
        RenderChainDX12 _chain;
        DisplayManagerDXGI _displayManager;
        DeviceBuilderDX12 _deviceBuilder;

        public RendererDX12()
        {
            
        }

        protected override void OnInitializeApi(GraphicsSettings settings)
        {
            _api = D3D12.GetApi();
            _chain = new RenderChainDX12(this);
            _deviceBuilder = new DeviceBuilderDX12(_api, this);
            _displayManager = new DisplayManagerDXGI(_deviceBuilder.GetCapabilities);
        }

        protected override void OnInitialize(EngineSettings settings)
        {
            base.OnInitialize(settings);

            Device = new DeviceDX12(settings.Graphics, _deviceBuilder, Log, _displayManager.SelectedAdapter);
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

        protected override void OnRebuildSurfaces(uint requiredWidth, uint requiredHeight)
        {
            throw new NotImplementedException();
        }

        protected override void OnDisposeBeforeRender()
        {
            Device?.Dispose();
            _api.Dispose();
        }

        internal DeviceDX12 Device { get; private set; }

        public override DisplayManager DisplayManager => _displayManager;

        public override ResourceFactory Resources { get; }

        public override IComputeManager Compute => throw new NotImplementedException();

        protected override IRenderChain Chain => _chain;
    }
}
