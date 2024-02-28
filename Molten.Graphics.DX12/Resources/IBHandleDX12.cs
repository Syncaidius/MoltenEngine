using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class IBHandleDX12 : ResourceHandleDX12
{
    IndexBufferView _view;

    internal unsafe IBHandleDX12(BufferDX12 buffer, ID3D12Resource1* resource) : 
        base(buffer, resource)
    {
        _view = new IndexBufferView()
        {
            BufferLocation = resource->GetGPUVirtualAddress() + buffer.Offset,
            Format = buffer.ResourceFormat.ToApi(),
            SizeInBytes = (uint)buffer.SizeInBytes,
        };
    }

    internal ref readonly IndexBufferView View => ref _view;
}
