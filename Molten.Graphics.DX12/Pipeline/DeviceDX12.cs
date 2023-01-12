using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics
{
    internal unsafe class DeviceDX12 : EngineObject
    {
        DeviceBuilderDX12 _builder;
        Logger _log;
        DisplayAdapterDXGI _adapter;
        DisplayManagerDXGI _displayManager;
        long _allocatedVRAM; 

        internal ID3D12Device10* Ptr;

        CommandQueueDX12 _qDirect;

        public DeviceDX12(GraphicsSettings graphics, DeviceBuilderDX12 deviceBuilder, Logger log, IDisplayAdapter selectedAdapter)
        {
            Settings = graphics;
            _builder = deviceBuilder;
            _log = log;
            _adapter = selectedAdapter as DisplayAdapterDXGI;
            _displayManager = _adapter.Manager as DisplayManagerDXGI;

            HResult r = _builder.CreateDevice(_adapter, out Ptr);
            if (!_builder.CheckResult(r, () => $"Failed to initialize {nameof(DeviceDX12)}"))
                return;

            CommandQueueDesc cmdDesc = new CommandQueueDesc()
            {
                Type = CommandListType.Direct
            };

            _qDirect = new CommandQueueDX12(_log, this, _builder, ref cmdDesc);
        }

        /// <summary>Track a VRAM allocation.</summary>
        /// <param name="bytes">The number of bytes that were allocated.</param>
        internal void AllocateVRAM(long bytes)
        {
            Interlocked.Add(ref _allocatedVRAM, bytes);
        }

        /// <summary>Track a VRAM deallocation.</summary>
        /// <param name="bytes">The number of bytes that were deallocated.</param>
        internal void DeallocateVRAM(long bytes)
        {
            Interlocked.Add(ref _allocatedVRAM, -bytes);
        }

        protected override void OnDispose()
        {
            _qDirect.Dispose();
            SilkUtil.ReleasePtr(ref Ptr);
        }

        internal DisplayManagerDXGI DisplayManager => _displayManager;

        internal DisplayAdapterDXGI Adapter => _adapter;

        internal GraphicsSettings Settings { get; }

        internal long AllocatedVRAM => _allocatedVRAM;
    }
}
