using Molten.Collections;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12
{
    internal unsafe class CommandAllocatorDX12 : GraphicsObject
    {
        ID3D12CommandAllocator* _handle;
        ThreadedList<CommandListDX12> _allocated;
        DeviceDX12 _device;

        public CommandAllocatorDX12(DeviceDX12 device, CommandListType type) : base(device)
        {
            _device = device;
            Guid guid = ID3D12CommandAllocator.Guid;
            Type = type;

            void* ptr = null;
            HResult hr = device.Ptr->CreateCommandAllocator(CommandListType.Direct, &guid, &ptr);
            if (!device.Log.CheckResult(hr, () => "Failed to create command allocator"))
                hr.Throw();

            _handle = (ID3D12CommandAllocator*)ptr;
            _allocated = new ThreadedList<CommandListDX12>();
        }

        private T* CreateCommandList<T>(Guid guid, ID3D12PipelineState* pInitialState /*TODO Properly provide a PipelineStateDX12*/)
            where T : unmanaged
        {
            void* ptr = null;
            HResult hr = _device.Ptr->CreateCommandList(0, Type, _handle, pInitialState, &guid, &ptr);
            if (!_device.Log.CheckResult(hr, () => $"Failed to allocate {Type} command list"))
                hr.Throw();

            return (T*)ptr;
        }

        internal GraphicsCommandListDX12 AllocateGraphics(ID3D12PipelineState* pInitialState /*TODO Properly provide a PipelineStateDX12*/)
        {
            ID3D12GraphicsCommandList* ptr = CreateCommandList<ID3D12GraphicsCommandList>(ID3D12GraphicsCommandList.Guid, pInitialState);
            GraphicsCommandListDX12 list = new GraphicsCommandListDX12(this, ptr);
            _allocated.Add(list);

            return list;
        }

        internal void Unallocate(CommandListDX12 cmd)
        {
            if(cmd.Allocator != this)
                throw new InvalidOperationException("Command list does not belong to this allocator.");

            _allocated.Remove(cmd);
        }

        protected override void OnGraphicsRelease()
        {
            _allocated.For(0, (index, cmd) => cmd.Dispose());
            SilkUtil.ReleasePtr(ref _handle);
        }

        public CommandListType Type { get; }
    }
}
