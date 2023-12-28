using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12
{
    internal unsafe class CommandListDX12 : GraphicsObject
    {
        ID3D12CommandList* _handle;

        internal CommandListDX12(CommandAllocatorDX12 allocator, void* handle) : base(allocator.Device)
        {
            Allocator = allocator;
            _handle = (ID3D12CommandList*)handle;
        }

        protected override void OnGraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _handle);
            Allocator.Unallocate(this);
        }

        /// <summary>
        /// Gets the parent <see cref="CommandAllocatorDX12"/> from which the current <see cref="CommandListDX12"/> was allocated.
        /// </summary>
        public CommandAllocatorDX12 Allocator { get; }
    }
}
