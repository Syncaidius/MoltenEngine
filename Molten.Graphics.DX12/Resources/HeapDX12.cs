using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12.Resources
{
    /// <summary>
    /// A heap for storing placed resources. Placed resources rely on the pre-allocated nature of heaps for quick allocation and deallocation.
    /// </summary>
    internal unsafe class HeapDX12 : GraphicsObject<DeviceDX12>
    {
        ID3D12Heap1* _handle;
        HeapDesc _desc;

        internal HeapDX12(DeviceDX12 device, HeapFlags flags, ulong capacity, HeapType type, MemoryPool memPool = MemoryPool.Unknown) : base(device)
        {
            Guid guid = ID3D12Heap1.Guid;
            void* ptr = null;

            _desc = new HeapDesc()
            {
                Alignment = 0,
                Flags = flags,
                SizeInBytes = capacity,
                Properties = new HeapProperties()
                {
                    VisibleNodeMask = 1U,
                    CPUPageProperty = CpuPageProperty.Unknown,
                    CreationNodeMask = 1U,
                    MemoryPoolPreference = memPool,
                    Type = type,
                },
            };

            HResult hr = device.Ptr->CreateHeap(_desc, &guid, &ptr);
            if (!device.Log.CheckResult(hr, () => $"Failed to create heap with capacity '{_desc.SizeInBytes}'"))
                return;

            _handle = (ID3D12Heap1*)ptr;
        }


        protected override void OnGraphicsRelease()
        {
            NativeUtil.ReleasePtr(ref _handle);
        }
    }
}
