using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D12;

namespace Molten.Graphics
{
    internal unsafe class DeviceDX12 : GraphicsDevice<ID3D12Device10>
    {
        DeviceBuilderDX12 _builder;
        DisplayAdapterDXGI _adapter;
        DisplayManagerDXGI _displayManager;

        CommandQueueDX12 _qDirect;

        public DeviceDX12(GraphicsSettings settings, DeviceBuilderDX12 deviceBuilder, Logger log, IDisplayAdapter adapter) : 
            base(settings, log, false)
        {
            _builder = deviceBuilder;
            _adapter = adapter as DisplayAdapterDXGI;
            _displayManager = _adapter.Manager as DisplayManagerDXGI;
        }

        protected override void OnInitialize()
        {
            HResult r = _builder.CreateDevice(_adapter, out PtrRef);
            if (!_builder.CheckResult(r, () => $"Failed to initialize {nameof(DeviceDX12)}"))
                return;

            CommandQueueDesc cmdDesc = new CommandQueueDesc()
            {
                Type = CommandListType.Direct
            };

            _qDirect = new CommandQueueDX12(Log, this, _builder, ref cmdDesc);
        }

        public override GraphicsDepthState CreateDepthState(GraphicsDepthState source = null)
        {
            throw new NotImplementedException();
        }

        public override GraphicsBlendState CreateBlendState(GraphicsBlendState source = null)
        {
            throw new NotImplementedException();
        }

        public override GraphicsRasterizerState CreateRasterizerState(GraphicsRasterizerState source = null)
        {
            throw new NotImplementedException();
        }

        protected override void OnDispose()
        {
            _qDirect.Dispose();
            base.OnDispose();
        }

        internal DisplayManagerDXGI DisplayManager => _displayManager;

        public override DisplayAdapterDXGI Adapter => _adapter;

        public override CommandQueueDX12 Cmd => _qDirect;
    }
}
