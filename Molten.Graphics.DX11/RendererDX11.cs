using System.Reflection;
using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics.DX11
{
    public class RendererDX11 : RenderService
    {
        D3D11 _api;
        GraphicsManagerDXGI _displayManager;
        DeviceBuilderDX11 _deviceBuilder;
        FxcCompiler _shaderCompiler;

        internal static Guid WKPDID_D3DDebugObjectName = new Guid(0x429b8c22, 0x9188, 0x4b0c, 0x87, 0x42, 0xac, 0xb0, 0xbf, 0x85, 0xc2, 0x00);

        protected unsafe override GraphicsManager OnInitializeDisplayManager(GraphicsSettings settings)
        {
            _api = D3D11.GetApi();
            _deviceBuilder = new DeviceBuilderDX11(_api, this, 
                D3DFeatureLevel.Level111,
                D3DFeatureLevel.Level110, 
                D3DFeatureLevel.Level101, 
                D3DFeatureLevel.Level100);
            _displayManager = new GraphicsManagerDXGI(CreateDevice, _deviceBuilder.GetCapabilities);

            return _displayManager;
        }

        private unsafe DeviceDXGI CreateDevice(GraphicsSettings settings, GraphicsManagerDXGI manager, IDXGIAdapter4* adapter)
        {
            return new DeviceDX11(this, manager, adapter, _deviceBuilder);
        }

        protected override GraphicsDevice OnInitializeDevice(GraphicsSettings settings, GraphicsManager manager)
        {
            NativeDevice = _displayManager.SelectedDevice as DeviceDX11;
            NativeDevice.Initialize();
            return NativeDevice;
        }

        protected override void OnInitializeRenderer(EngineSettings settings)
        {
            Assembly includeAssembly = GetType().Assembly;
            
            _shaderCompiler = new FxcCompiler(this, Log, "\\Assets\\HLSL\\include\\", includeAssembly);
        }

        protected override void OnDisposeBeforeRender()
        {
            _displayManager.Dispose();
            NativeDevice?.Dispose();
            _api.Dispose();
        }

        internal DeviceDX11 NativeDevice { get; private set; }

        public override FxcCompiler Compiler => _shaderCompiler;
    }
}
