using Molten.Collections;
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

        RasterizerStateBank _rasterizerBank;
        BlendStateBank _blendBank;
        DepthStateBank _depthBank;
        SamplerBank _samplerBank;

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
            VertexFormatCache = new TypedObjectCache<IVertexType, VertexFormat>(VertexFormat.FromType);
            _bufferSegmentPool = new ObjectPool<BufferSegment>(() => new BufferSegment(this));

            HResult r = _builder.CreateDevice(_adapter, out PtrRef, out ID3D11DeviceContext4* deviceContext);
            if (r.IsFailure)
            {
                Log.Error($"Failed to initialize {nameof(DeviceDX11)}. Code: {r}");
                return;
            }

            CmdList = new CommandQueueDX11(this, deviceContext);

            _rasterizerBank = new RasterizerStateBank(this);
            _blendBank = new BlendStateBank(this);
            _depthBank = new DepthStateBank(this);
            _samplerBank = new SamplerBank(this);
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

        /// <summary>Disposes of the <see cref="DeviceDX11"/> and any deferred contexts and resources bound to it.</summary>
        protected override void OnDispose()
        {
            for (int i = _deferredContexts.Count - 1; i >= 0; i--)
                RemoveDeferredContext(_deferredContexts[i]);

            // TODO dispose of all bound IGraphicsResource
            VertexFormatCache.Dispose();
            RasterizerBank.Dispose();
            BlendBank.Dispose();
            DepthBank.Dispose();
            SamplerBank.Dispose();

            CmdList.Dispose();
            _bufferSegmentPool.Dispose();

            base.OnDispose();
        }

        internal DisplayManagerDXGI DisplayManager => _displayManager;

        public override DisplayAdapterDXGI Adapter => _adapter;

        internal TypedObjectCache<IVertexType, VertexFormat> VertexFormatCache { get; }

        /// <summary>
        /// Gets the device's blend state bank.
        /// </summary>
        internal BlendStateBank BlendBank => _blendBank;

        /// <summary>
        /// Gets the device's rasterizer state bank.
        /// </summary>
        internal RasterizerStateBank RasterizerBank => _rasterizerBank;

        /// <summary>
        /// Gets the device's depth-stencil state bank.
        /// </summary>
        internal DepthStateBank DepthBank => _depthBank;

        /// <summary>
        /// Gets the device's texture sampler bank.
        /// </summary>
        internal SamplerBank SamplerBank => _samplerBank;

        /// <summary>
        /// The main <see cref="CommandQueueDX11"/> of the current <see cref="DeviceDX11"/>. This is used for issuing immediate commands to the GPU.
        /// </summary>
        internal CommandQueueDX11 Cmd => CmdList;
    }
}
