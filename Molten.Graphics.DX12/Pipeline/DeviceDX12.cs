﻿using Molten.Graphics.Dxgi;
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

        protected override ShaderSampler OnCreateSampler(ref ShaderSamplerParameters parameters)
        {
            throw new NotImplementedException();
        }

        public override HlslPass CreateShaderPass(HlslShader shader, string name = null)
        {
            throw new NotImplementedException();
        }

        public override IVertexBuffer CreateVertexBuffer<T>(BufferMode mode, uint numVertices, T[] initialData = null)
        {
            throw new NotImplementedException();
        }

        public override IIndexBuffer CreateIndexBuffer(BufferMode mode, uint numIndices, ushort[] initialData = null)
        {
            throw new NotImplementedException();
        }

        public override IIndexBuffer CreateIndexBuffer(BufferMode mode, uint numIndices, uint[] initialData = null)
        {
            throw new NotImplementedException();
        }

        public override IStructuredBuffer CreateStructuredBuffer<T>(BufferMode mode, uint numElements, bool allowUnorderedAccess, bool isShaderResource, T[] initialData = null)
        {
            throw new NotImplementedException();
        }

        public override IStagingBuffer CreateStagingBuffer(StagingBufferFlags staging, uint byteCapacity)
        {
            throw new NotImplementedException();
        }

        protected override void OnDispose()
        {
            _qDirect.Dispose();
            base.OnDispose();
        }

        public override DisplayManagerDXGI DisplayManager => _displayManager;

        public override DisplayAdapterDXGI Adapter => _adapter;

        public override CommandQueueDX12 Cmd => _qDirect;
    }
}
