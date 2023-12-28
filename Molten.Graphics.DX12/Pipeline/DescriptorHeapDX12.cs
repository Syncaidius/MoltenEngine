using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12
{
    internal unsafe class DescriptorHeapDX12
    {
        internal delegate void IterateCallback<T>(ref T handle) where T : unmanaged;

        ID3D12DescriptorHeap* _handle;
        DescriptorHeapDesc _desc;
        CpuDescriptorHandle _cpuStartHandle;
        GpuDescriptorHandle _gpuStartHandle;

        internal DescriptorHeapDX12(DeviceDX12 device, uint capacity, DescriptorHeapType type, DescriptorHeapFlags flags)
        {
            _desc = new DescriptorHeapDesc()
            {
                NodeMask = 0,
                Type = type,
                Flags = flags,
                NumDescriptors = capacity,
            };

            Guid guid = ID3D12DescriptorHeap.Guid;
            void* ptr = null;

            HResult hr;
            fixed (DescriptorHeapDesc* ptrDesc = &_desc)
                hr = device.Ptr->CreateDescriptorHeap(ptrDesc, &guid, &ptr);

            if(!device.Log.CheckResult(hr, () => $"Failed to create descriptor heap with capacity '{capacity}'"))
                return;

            _handle = (ID3D12DescriptorHeap*)ptr;
            _cpuStartHandle = _handle->GetCPUDescriptorHandleForHeapStart();
            IncrementSize = device.Ptr->GetDescriptorHandleIncrementSize(type);

            // Only create a GPU start handle if the heap is shader visible.
            if(flags.HasFlag(DescriptorHeapFlags.ShaderVisible))
                _gpuStartHandle = _handle->GetGPUDescriptorHandleForHeapStart();
        }

        internal void IterateForCpu(IterateCallback<CpuDescriptorHandle> callback)
        {
            CpuDescriptorHandle handle = _cpuStartHandle;
            for(int i = 0; i < _desc.NumDescriptors; i++)
            {
                callback(ref handle);   
                handle.Ptr += IncrementSize;
            }
        }

        internal void IterateForGpu(IterateCallback<GpuDescriptorHandle> callback)
        {
            if (!_desc.Flags.HasFlag(DescriptorHeapFlags.ShaderVisible))
                throw new InvalidOperationException("Cannot iterate as GPU descriptor handles without being visible to shaders.");

            GpuDescriptorHandle handle = _gpuStartHandle;
            for (int i = 0; i < _desc.NumDescriptors; i++)
            {
                callback(ref handle);
                handle.Ptr += IncrementSize;
            }
        }

        internal CpuDescriptorHandle GetCpuHandle(uint index)
        {
            return new CpuDescriptorHandle(_cpuStartHandle.Ptr + (index * IncrementSize));
        }

        internal GpuDescriptorHandle GetGpuHandle(uint index)
        {
            return new GpuDescriptorHandle(_gpuStartHandle.Ptr + (index * IncrementSize));
        }

        public void Dispose()
        {
           SilkUtil.ReleasePtr(ref _handle);
        }

        public ref readonly uint Capacity => ref _desc.NumDescriptors;

        public ref readonly DescriptorHeapType Type => ref _desc.Type;

        public ref readonly DescriptorHeapFlags Flags => ref _desc.Flags;

        public uint IncrementSize { get; }
    }
}
