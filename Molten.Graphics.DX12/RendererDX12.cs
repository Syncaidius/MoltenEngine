using Molten.Graphics.Dxgi;
using Silk.NET.Direct3D12;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    public class RendererDX12 : RenderService
    {
        D3D12 _api;
        DisplayManagerDXGI _displayManager;

        public RendererDX12()
        {
            
        }

        protected override unsafe GraphicsDisplayManager OnInitializeDisplayManager(GraphicsSettings settings)
        {
            _api = D3D12.GetApi();
            Builder = new DeviceBuilderDX12(_api, this);
            _displayManager = new DisplayManagerDXGI(CreateDevice, Builder.GetCapabilities);
            return _displayManager;
        }

        private unsafe GraphicsDeviceDXGI CreateDevice(GraphicsSettings settings, DisplayManagerDXGI manager, IDXGIAdapter4* adapter)
        {
            return new DeviceDX12(this, _displayManager, adapter, Builder);
        }


        protected override GraphicsDevice OnInitializeDevice(GraphicsSettings settings, GraphicsDisplayManager manager)
        {
            NativeDevice = _displayManager.SelectedDevice as DeviceDX12;
            NativeDevice.Initialize();
            return NativeDevice;
        }

        protected override void OnInitializeRenderer(EngineSettings settings)
        {
            
        }

        protected override void OnPostPresent(Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnPrePresent(Timing time)
        {
            throw new NotImplementedException();
        }

        protected override void OnDisposeBeforeRender()
        {
            NativeDevice?.Dispose();
            _api.Dispose();
        }

        internal DeviceDX12 NativeDevice { get; private set; }

        internal DeviceBuilderDX12 Builder { get; private set; }

        public override DxcCompiler Compiler { get; }
    }
}
