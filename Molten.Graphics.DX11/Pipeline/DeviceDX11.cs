using Molten.Collections;
using Molten.Graphics.Dxgi;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;

namespace Molten.Graphics
{
    /// <summary>A Direct3D 11 graphics device.</summary>
    /// <seealso cref="DeviceContext" />
    public unsafe class DeviceDX11 : DeviceContext
    {
        internal ID3D11Device5* NativeDevice;
        internal ID3D11DeviceContext4* ImmediateContext;

        DeviceBuilderDX11 _builder;
        DisplayAdapterDXGI _adapter;
        List<DeviceContext> _contexts;

        Logger _log;
        DisplayManagerDXGI _displayManager;
        long _allocatedVRAM;

        RasterizerStateBank _rasterizerBank;
        BlendStateBank _blendBank;
        DepthStateBank _depthBank;
        SamplerBank _samplerBank;

        ObjectPool<BufferSegment> _bufferSegmentPool;
        ThreadedQueue<ContextObject> _objectsToDispose;

        /// <summary>The adapter to initially bind the graphics device to. Can be changed later.</summary>
        /// <param name="adapter">The physical display adapter to bind the new device to.</param>
        internal DeviceDX11(GraphicsSettings settings, DeviceBuilderDX11 builder, Logger log, IDisplayAdapter adapter)
        {
            _builder = builder;
            _log = log;
            _displayManager = adapter.Manager as DisplayManagerDXGI;
            _adapter = adapter as DisplayAdapterDXGI;
            _contexts = new List<DeviceContext>();
            Contexts = _contexts.AsReadOnly();
            Settings = settings;
            VertexFormatCache = new TypedObjectCache<IVertexType, VertexFormat>(VertexFormat.FromType);

            _bufferSegmentPool = new ObjectPool<BufferSegment>(() => new BufferSegment(this));
            _objectsToDispose = new ThreadedQueue<ContextObject>();

            HResult r = _builder.CreateDevice(_adapter, out NativeDevice, out ImmediateContext);

            _rasterizerBank = new RasterizerStateBank(this);
            _blendBank = new BlendStateBank(this);
            _depthBank = new DepthStateBank(this);
            _samplerBank = new SamplerBank(this);

            Initialize(_log, this, ImmediateContext);
        }

        internal BufferSegment GetBufferSegment()
        {
            return _bufferSegmentPool.GetInstance();
        }

        internal void RecycleBufferSegment(BufferSegment segment)
        {
            _bufferSegmentPool.Recycle(segment);
        }

        public void MarkForRelease(ContextObject pObject)
        {
            if (IsDisposed)
                pObject.PipelineRelease();
            else
                _objectsToDispose.Enqueue(pObject);
        }

        internal void DisposeMarkedObjects()
        {
            while (_objectsToDispose.TryDequeue(out ContextObject obj))
                obj.PipelineRelease();
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

        /// <summary>Gets a new deferred <see cref="DeviceContext"/>.</summary>
        /// <returns></returns>
        internal DeviceContext GetDeferredContext()
        {
            ID3D11DeviceContext3* dc = null;
            NativeDevice->CreateDeferredContext3(0, &dc);

            Guid cxt4Guid = ID3D11DeviceContext4.Guid;
            void* ptr4 = null;
            dc->QueryInterface(&cxt4Guid, &ptr4);

            DeviceContext context = new DeviceContext();
            context.Initialize(_log, this, (ID3D11DeviceContext4*)ptr4);
            _contexts.Add(context);
            return context;
        }

        internal void RemoveDeferredContext(DeviceContext context)
        {
            if(context == this)
                throw new GraphicsContextException("Cannot remove the graphics device from itself.");

            if (context.Device != this)
                throw new GraphicsContextException("Graphics pipe is owned by another device.");

            if (!context.IsDisposed)
                context.Dispose();

            _contexts.Remove(context);
        }

        internal void SubmitContext(DeviceContext context)
        {
            if (context.Type != GraphicsContextType.Deferred)
                throw new Exception("Cannot submit immediate graphics contexts, only deferred.");

            // TODO take the underlying DX context from the GraphicsContext and give it a new/recycled one to work with.
            // TODO add the context's profiler stats to the device's main profiler.
        }

        /// <summary>Disposes of the <see cref="DeviceDX11"/> and any deferred contexts and resources bound to it.</summary>
        protected override void OnDispose()
        {
            for (int i = _contexts.Count - 1; i >= 0; i--)
                RemoveDeferredContext(_contexts[i]);

            // TODO dispose of all bound IGraphicsResource
            VertexFormatCache.Dispose();
            RasterizerBank.Dispose();
            BlendBank.Dispose();
            DepthBank.Dispose();
            SamplerBank.Dispose();

            SilkUtil.ReleasePtr(ref ImmediateContext);
            SilkUtil.ReleasePtr(ref NativeDevice);

            _bufferSegmentPool.Dispose();
            DisposeMarkedObjects();

            base.OnDispose();
        }

        internal IReadOnlyCollection<DeviceContext> Contexts { get; }

        internal DisplayManagerDXGI DisplayManager => _displayManager;

        internal DisplayAdapterDXGI Adapter => _adapter;

        internal GraphicsSettings Settings { get; }

        internal TypedObjectCache<IVertexType, VertexFormat> VertexFormatCache { get; }

        internal long AllocatedVRAM => _allocatedVRAM;

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
    }
}
