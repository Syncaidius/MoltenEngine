﻿using Molten.Collections;
using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>A Direct3D 11 graphics device.</summary>
    /// <seealso cref="CommandQueueDX11" />
    public unsafe class DeviceDX11 : GraphicsDevice<ID3D11Device5>
    {
        DeviceBuilderDX11 _builder;
        DisplayAdapterDXGI _adapter;
        DisplayManagerDXGI _displayManager;

        ObjectPool<BufferSegment> _bufferSegmentPool;

        CommandQueueDX11 CmdList;
        List<CommandQueueDX11> _deferredContexts;

        /// <summary>The adapter to initially bind the graphics device to. Can be changed later.</summary>
        /// <param name="adapter">The physical display adapter to bind the new device to.</param>
        internal DeviceDX11(GraphicsSettings settings, DeviceBuilderDX11 builder, Logger log, IDisplayAdapter adapter) :
            base(settings, log, false)
        {
            _builder = builder;
            _displayManager = adapter.Manager as DisplayManagerDXGI;
            _adapter = adapter as DisplayAdapterDXGI;
            _deferredContexts = new List<CommandQueueDX11>();

            VertexFormatCache = new VertexFormatCache<ShaderIOStructureDX11>(
                (elementCount) => new ShaderIOStructureDX11(elementCount),
                (att, structure, index, byteOffset) =>
                {
                    ShaderIOStructureDX11 dxStruct = structure as ShaderIOStructureDX11;

                    dxStruct.Elements[index] = new InputElementDesc()
                    {
                        SemanticName = (byte*)SilkMarshal.StringToPtr(dxStruct.Metadata[index].Name),
                        SemanticIndex = att.SemanticIndex,
                        AlignedByteOffset = byteOffset,
                        InputSlotClass = att.Classification.ToApi(),
                        Format = att.Type.ToGraphicsFormat().ToApi()
                    };
                });

            _bufferSegmentPool = new ObjectPool<BufferSegment>(() => new BufferSegment(this));
        }

        protected override void OnInitialize()
        {
            HResult r = _builder.CreateDevice(_adapter, out PtrRef, out ID3D11DeviceContext4* deviceContext);
            if (r.IsFailure)
            {
                Log.Error($"Failed to initialize {nameof(DeviceDX11)}. Code: {r}");
                return;
            }

            CmdList = new CommandQueueDX11(this, deviceContext);
        }

        internal BufferSegment GetBufferSegment()
        {
            return _bufferSegmentPool.GetInstance();
        }

        internal void RecycleBufferSegment(BufferSegment segment)
        {
            _bufferSegmentPool.Recycle(segment);
        }

        /// <summary>Gets a new deferred <see cref="CommandQueueDX11"/>.</summary>
        /// <returns></returns>
        internal CommandQueueDX11 GetDeferredContext()
        {
            ID3D11DeviceContext3* dc = null;
            Ptr->CreateDeferredContext3(0, &dc);

            Guid cxt4Guid = ID3D11DeviceContext4.Guid;
            void* ptr4 = null;
            dc->QueryInterface(&cxt4Guid, &ptr4);

            CommandQueueDX11 context = new CommandQueueDX11(this, (ID3D11DeviceContext4*)ptr4);
            _deferredContexts.Add(context);
            return context;
        }

        internal void RemoveDeferredContext(CommandQueueDX11 cmd)
        {
            if (cmd.DXDevice != this)
                throw new GraphicsCommandQueueException(cmd, "Graphics pipe is owned by another device.");

            if (!cmd.IsDisposed)
                cmd.Dispose();

            _deferredContexts.Remove(cmd);
        }

        internal void SubmitContext(CommandQueueDX11 context)
        {
            if (context.Type != GraphicsContextType.Deferred)
                throw new Exception("Cannot submit immediate graphics contexts, only deferred.");

            // TODO take the underlying DX context from the GraphicsContext and give it a new/recycled one to work with.
            // TODO add the context's profiler stats to the device's main profiler.
        }

        public override GraphicsDepthState CreateDepthState(GraphicsDepthState source = null)
        {
            return new DepthStateDX11(this, source as DepthStateDX11);
        }

        public override GraphicsBlendState CreateBlendState(GraphicsBlendState source = null)
        {
            return new BlendStateDX11(this, source as BlendStateDX11);
        }

        public override GraphicsRasterizerState CreateRasterizerState(GraphicsRasterizerState source = null)
        {
            return new RasterizerStateDX11(this, source as RasterizerStateDX11);
        }

        public override ShaderSampler CreateSampler(ShaderSampler source = null)
        {
            return new ShaderSamplerDX11(this, source as ShaderSamplerDX11);
        }

        public override ShaderComposition CreateShaderComposition(ShaderType type, HlslShader parent)
        {
            switch (type)
            {
                case ShaderType.Vertex: return new VSComposition(parent);
                case ShaderType.Hull: return new HSComposition(parent);
                case ShaderType.Domain: return new DSComposition(parent);
                case ShaderType.Geometry: return new GSComposition(parent);
                case ShaderType.Pixel: return new PSComposition(parent);
                case ShaderType.Compute: return null;
                default: return null;
            }
        }

        /// <summary>Disposes of the <see cref="DeviceDX11"/> and any deferred contexts and resources bound to it.</summary>
        protected override void OnDispose()
        {
            for (int i = _deferredContexts.Count - 1; i >= 0; i--)
                RemoveDeferredContext(_deferredContexts[i]);

            // TODO dispose of all bound IGraphicsResource
            VertexFormatCache.Dispose();
            BlendBank.Dispose();
            _bufferSegmentPool.Dispose();

            base.OnDispose();
        }

        public override DisplayManagerDXGI DisplayManager => _displayManager;

        public override DisplayAdapterDXGI Adapter => _adapter;

        internal VertexFormatCache<ShaderIOStructureDX11> VertexFormatCache { get; }

        /// <inheritdoc/>
        public override CommandQueueDX11 Cmd => CmdList;
    }
}
