using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    using Direct2DFactory = SharpDX.Direct2D1.Factory;
    using DirectWriteFactory = SharpDX.DirectWrite.Factory;
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
        RenderProfilerDX _mainProfiler;
        VertexFormatBuilder _vertexBuilder;

        ImagingFactory _wicFactory;
        Direct2DFactory _d2dFactory;
        DirectWriteFactory _dWriteFactory;
        DX11DisplayManager _displayManager;
        GraphicsSettings _settings;
        ShaderSampler _defaultSampler;

        /// <summary>The adapter to initially bind the graphics device to. Can be changed later.</summary>
        /// <param name="adapter">The adapter.</param>
        internal GraphicsDevice(Logger log, GraphicsSettings settings, RenderProfilerDX profiler, DX11DisplayManager manager, bool enableDebugLayer)
        {
            _log = log;
            _displayManager = manager;
            _adapter = _displayManager.SelectedAdapter as GraphicsAdapter<Adapter1, AdapterDescription1, Output1>;
            _contexts = new ThreadedList<GraphicsPipe>();
            _mainProfiler = profiler;
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
            Initialize(_log, this, _d3d.ImmediateContext);

            _wicFactory = new ImagingFactory();
            _d2dFactory = new Direct2DFactory(SharpDX.Direct2D1.FactoryType.SingleThreaded);
            _dWriteFactory = new DirectWriteFactory(SharpDX.DirectWrite.FactoryType.Shared);

            //// Initialize surfaces for the initial outputs.
            //List<IDisplayOutput> initialOutputs = new List<IDisplayOutput>();
            //_adapter.GetActiveOutputs(initialOutputs);
            //foreach (GraphicsOutput output in initialOutputs)
            //    CreateOutputSurface(output);

            //_adapter.OnOutputAdded += _adapter_OnOutputAdded;
            //_adapter.OnOutputRemoved += _adapter_OnOutputRemoved;

            CreateDefaultResources();
            ExternalContext = this;
        }

        private void CreateDefaultResources()
        {
            _defaultSampler = new ShaderSampler(this, false);
        }

        /// <summary>Gets a new deferred <see cref="GraphicsPipe"/>.</summary>
        /// <returns></returns>
        public GraphicsPipe GetDeferredContext()
        {
            GraphicsPipe context = new GraphicsPipe();
            context.Initialize(_log, this, new DeviceContext(_d3d));
            _contexts.Add(context);
            return context;
        }

        public bool RemoveContext(GraphicsPipe context)
        {
            if(context == this)
                throw new GraphicsContextException("Cannot remove the graphics device from itself.");

            return _contexts.Remove(context);
        }

        public void SubmitContext(GraphicsPipe context)
        {
            if (context.Type != GraphicsContextType.Deferred)
                throw new Exception("Cannot submit immediate graphics contexts, only deferred.");

            // TODO take the underlying DX context from the GraphicsContext and give it a new/recycled one to work with.
            // TODO add the context's profiler stats to the device's main profiler.
        }

        /// <summary>Disposes of the <see cref="GraphicsDevice"/> and any deferred contexts and resources bound to it.</summary>
        public override void Dispose()
        {
            // TODO dispose of all bound IGraphicsResource

            _contexts.ForInterlock(0, 1, (index, context) =>
            {
                context.Dispose();
                return false;
            });

            _contexts.Clear();

            DisposeObject(ref _defaultSampler);
            DisposeObject(ref _wicFactory);
            DisposeObject(ref _d2dFactory);
            DisposeObject(ref _dWriteFactory);
            DisposeObject(ref _d3d);
        }

        /// <summary>Gets the underlying D3D device.</summary>
        internal Device D3d => _d3d;

        internal ThreadedList<GraphicsPipe> ActiveContexts => _contexts;

        /// <summary>Gets an instance of <see cref="GraphicsDeviceFeatures"/> which provides access to feature support details for the current graphics device.</summary>
        public GraphicsDeviceFeatures Features => _features;

        /// <summary>Gets the main profiler bound to the device.</summary>
        internal RenderProfilerDX MainProfiler => _mainProfiler;

        /// <summary>Returns the Windows Imaging Component (WIC) factory.</summary>
        public ImagingFactory WICFactory => _wicFactory;

        /// <summary>Gets the Direct2D factory instance.</summary>
        public Direct2DFactory Direct2D => _d2dFactory;

        /// <summary>Gets the DirectWrite factory instance.</summary>
        public DirectWriteFactory DirectWrite => _dWriteFactory;

        public DX11DisplayManager DisplayManager => _displayManager;

        public GraphicsSettings Settings => _settings;

        /// <summary>Gets or sets the default render surface. This is the surface that <see cref="GraphicsPipe"/> instances revert to
        /// when a render surface is set to null.</summary>
        internal RenderSurfaceBase DefaultSurface { get; set; }

        internal VertexFormatBuilder VertexBuilder => _vertexBuilder;

        /// <summary>Gets the default shader sampler.</summary>
        internal ShaderSampler DefaultSampler
        {
            get => _defaultSampler;
        }
    }
}
