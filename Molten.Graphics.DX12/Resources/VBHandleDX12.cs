using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class VBHandleDX12 : ResourceHandleDX12
{
    VertexBufferView _view;

    internal unsafe VBHandleDX12(BufferDX12 buffer, ID3D12Resource1* resource) : 
        base(buffer, resource)
    {
        _view = new VertexBufferView()
        {
            BufferLocation = resource->GetGPUVirtualAddress() + buffer.Offset,
            SizeInBytes = (uint)buffer.SizeInBytes,
            StrideInBytes = buffer.Stride,
        };
    }

    internal ref VertexBufferView View => ref _view;
}
