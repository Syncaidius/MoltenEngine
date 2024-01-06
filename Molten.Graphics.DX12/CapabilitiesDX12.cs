using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal class CapabilitiesDX12
{
    /// <summary>
    /// Gets the highest supported root signature version for the current device.
    /// </summary>
    public D3DRootSignatureVersion RootSignatureVersion { get; internal set; }
}
