using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;

internal unsafe class PipelineStateDX12 : GraphicsObject<DeviceDX12>
{
    ID3D12PipelineState* _handle;

    /// <summary>
    /// Creates a new instance of <see cref="PipelineStateDX12"/>.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="handle"></param>
    public PipelineStateDX12(DeviceDX12 device, ID3D12PipelineState* handle, RootSignatureDX12 rootSig) : 
        base(device)
    {
        _handle = handle;
        RootSignature = rootSig;
    }

    public CachedPipelineState GetCachedBlob()
    {
        ID3D10Blob* blob;
        _handle->GetCachedBlob(&blob);

        return new CachedPipelineState()
        {
           PCachedBlob = blob,
           CachedBlobSizeInBytes = blob->GetBufferSize(),
        };
    }

    protected override void OnGraphicsRelease()
    {
        RootSignature?.Dispose(true);
        NativeUtil.ReleasePtr(ref _handle);
    }

    internal ID3D12PipelineState* Handle => _handle;

    internal RootSignatureDX12 RootSignature { get; }
}
