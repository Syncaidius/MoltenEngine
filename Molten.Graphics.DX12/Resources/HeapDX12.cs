using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12.Resources
{
    /// <summary>
    /// A heap for storing placed resources. Placed resources rely on the pre-allocated nature of heaps for quick allocation and deallocation.
    /// </summary>
    internal unsafe class HeapDX12 : GraphicsObject<DeviceDX12>
    {
        ID3D12Heap1* _handle;

        protected override void OnGraphicsRelease()
        {
            
        }
    }
}
