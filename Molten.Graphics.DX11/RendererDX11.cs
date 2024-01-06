using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System.Reflection;

namespace Molten.Graphics.DX11;

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

    protected override List<GraphicsDevice> OnInitializeDevices(GraphicsSettings settings, GraphicsManager manager)
    {
        // Initialize the primary device first.
        List<GraphicsDevice> result = new List<GraphicsDevice>();
        NativeDevice = _displayManager.PrimaryDevice as DeviceDX11;
        NativeDevice.Initialize();
        result.Add(NativeDevice);

        // Initialize secondary devices.
        foreach(GraphicsDevice device in _displayManager.Devices)
        {
            DeviceDX11 dxDevice = device as DeviceDX11;
            if (dxDevice == NativeDevice || 
                dxDevice.Type == GraphicsDeviceType.Cpu || 
                dxDevice.Type == GraphicsDeviceType.Other)
                continue;

            dxDevice.Initialize();
            result.Add(dxDevice);
        }

        return result;
    }

    protected override void OnInitializeRenderer(EngineSettings settings)
    {
        Assembly includeAssembly = GetType().Assembly;       
        _shaderCompiler = new FxcCompiler(this, Log, "\\Assets\\HLSL\\include\\", includeAssembly);
    }

    protected override void OnDisposeBeforeRender()
    {
        _shaderCompiler?.Dispose();
        _displayManager?.Dispose();
        _api?.Dispose();
    }

    internal DeviceDX11 NativeDevice { get; private set; }

    protected override ShaderCompiler Compiler => _shaderCompiler;
}
