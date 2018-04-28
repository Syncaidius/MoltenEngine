using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Molten.Graphics
{
    using Device = SharpDX.Direct3D11.Device;

    /// <summary>A Direct3D 11 graphics device.</summary>
    /// <seealso cref="Molten.Graphics.GraphicsPipe" />
    internal class GraphicsDevice : GraphicsPipe
    {
        /// <summary>Gets the pipe/context that is to be used by callers outside of the renderer.</summary>
        internal GraphicsPipe ExternalContext;

        Device _d3d;
        GraphicsAdapter<Adapter1, AdapterDescription1, Output1> _adapter;
        List<SwapChainSurface> _swapChains;
        GraphicsDeviceFeatures _features;
        ThreadedList<GraphicsPipe> _contexts;
        Logger _log;
        VertexFormatBuilder _vertexBuilder;
        DX11DisplayManager _displayManager;
        GraphicsSettings _settings;
        long _allocatedVRAM;

        RasterizerStateBank _rasterizerBank;
        BlendStateBank _blendBank;
        DepthStateBank _depthBank;
        SamplerBank _samplerBank;

        /// <summary>The adapter to initially bind the graphics device to. Can be changed later.</summary>
        /// <param name="adapter">The adapter.</param>
        internal GraphicsDevice(Logger log, GraphicsSettings settings, RenderProfilerDX profiler, DX11DisplayManager manager, bool enableDebugLayer)
        {
            _log = log;
            _displayManager = manager;
            _adapter = _displayManager.SelectedAdapter as GraphicsAdapter<Adapter1, AdapterDescription1, Output1>;
            _contexts = new ThreadedList<GraphicsPipe>();
            _swapChains = new List<SwapChainSurface>();
            _vertexBuilder = new VertexFormatBuilder();
            _settings = settings;

            DeviceCreationFlags flags = DeviceCreationFlags.BgraSupport;

            if (enableDebugLayer)
            {
                _log.WriteLine("Renderer debug layer enabled");
                flags |= DeviceCreationFlags.Debug;
            }

            using (var defaultDevice = new Device(_adapter.Adapter, flags, FeatureLevel.Level_11_0))
                _d3d = defaultDevice.QueryInterface<Device>();

            _features = new GraphicsDeviceFeatures(_d3d);

            _rasterizerBank = new RasterizerStateBank();
            _blendBank = new BlendStateBank();
            _depthBank = new DepthStateBank();
            _samplerBank = new SamplerBank();

            Initialize(_log, this, _d3d.ImmediateContext);
            ExternalContext = this;
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

        /// <summary>Gets a new deferred <see cref="GraphicsPipe"/>.</summary>
        /// <returns></returns>
        internal GraphicsPipe GetDeferredContext()
        {
            GraphicsPipe context = new GraphicsPipe();
            context.Initialize(_log, this, new DeviceContext(_d3d));
            _contexts.Add(context);
            return context;
        }

        internal bool RemoveContext(GraphicsPipe context)
        {
            if(context == this)
                throw new GraphicsContextException("Cannot remove the graphics device from itself.");

            return _contexts.Remove(context);
        }

        internal void SubmitContext(GraphicsPipe context)
        {
            if (context.Type != GraphicsContextType.Deferred)
                throw new Exception("Cannot submit immediate graphics contexts, only deferred.");

            // TODO take the underlying DX context from the GraphicsContext and give it a new/recycled one to work with.
            // TODO add the context's profiler stats to the device's main profiler.
        }

        /// <summary>Disposes of the <see cref="GraphicsDevice"/> and any deferred contexts and resources bound to it.</summary>
        protected override void OnDispose()
        {
            // TODO dispose of all bound IGraphicsResource

            _contexts.ForInterlock(0, 1, (index, context) =>
            {
                context.Dispose();
                return false;
            });

            _contexts.Clear();

            DisposeObject(ref _rasterizerBank);
            DisposeObject(ref _blendBank);
            DisposeObject(ref _depthBank);
            DisposeObject(ref _samplerBank);
            DisposeObject(ref _d3d);

            base.OnDispose();
        }

        /// <summary>Gets the underlying D3D device.</summary>
        internal Device D3d => _d3d;

        internal ThreadedList<GraphicsPipe> ActiveContexts => _contexts;

        /// <summary>Gets an instance of <see cref="GraphicsDeviceFeatures"/> which provides access to feature support details for the current graphics device.</summary>
        internal GraphicsDeviceFeatures Features => _features;

        internal DX11DisplayManager DisplayManager => _displayManager;

        internal GraphicsSettings Settings => _settings;

        /// <summary>Gets or sets the default render surface. This is the surface that <see cref="GraphicsPipe"/> instances revert to
        /// when a render surface is set to null.</summary>
        internal RenderSurfaceBase DefaultSurface { get; set; }

        internal VertexFormatBuilder VertexBuilder => _vertexBuilder;

        internal long AllocatedVRAM => _allocatedVRAM;

        /// <summary>
        /// Gets the device's blend state bank.
        /// </summary>
        public BlendStateBank BlendBank => _blendBank;

        /// <summary>
        /// Gets the device's rasterizer state bank.
        /// </summary>
        public RasterizerStateBank RasterizerBank => _rasterizerBank;

        /// <summary>
        /// Gets the device's depth-stencil state bank.
        /// </summary>
        public DepthStateBank DepthBank => _depthBank;

        /// <summary>
        /// Gets the device's texture sampler bank.
        /// </summary>
        public SamplerBank SamplerBank => _samplerBank;
    }
}
