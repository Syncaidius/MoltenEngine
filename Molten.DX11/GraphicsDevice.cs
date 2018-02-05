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

        /*  
            // MOVING SCENE GRAPH FROM UPDATE/CPU to RENDER/GPU:
            21:04 - Zeracronius: mmm
            21:04 - Zeracronius: if you did to a pre-defined frame time
            21:04 - Zeracronius: in which systems can add to specific frame
            21:05 - Zeracronius: what would potential consequences be...
            21:05 - Zeracronius: worst one i can think of
            21:05 - Zeracronius: is if something takes so long
            21:05 - Zeracronius: that it isn't ready and so is commited every 2nd frame
            21:05 - Zeracronius: so
            21:05 - Zeracronius: flashing objects
            21:06 - Syncaidius: well
            21:06 - Syncaidius: one mistake i made in my previous attempt
            21:06 - Syncaidius: was i had the scene graph cpu/update side
            21:06 - Syncaidius: not gpu/render side
            21:06 - Syncaidius: the way its meant to be
            21:06 - Syncaidius: is the update ticks tell the renderer
            21:06 - Syncaidius: where to move an object next
            21:06 - Syncaidius: or when to add/remove them
            21:06 - Syncaidius: soo
            21:06 - Syncaidius: if an update thread misses a frame with that design
            21:06 - Zeracronius: yeah
            21:06 - Syncaidius: it simply doesnt move
            21:06 - Syncaidius: but still renders
            21:07 - Syncaidius: the way i had it previously
            21:07 - Syncaidius: if the update thread missed something
            21:07 - Syncaidius: it vanished
            21:07 - Syncaidius: xD
            21:07 - Zeracronius: which means that the FPS itself is purely GPU interaction
            21:07 - Zeracronius: nothing logic

            // RENDER THREAD SYNCING + INTERPOLATION
            21:06 - Syncaidius: it simply doesnt move
            21:06 - Syncaidius: but still renders
            21:07 - Syncaidius: the way i had it previously
            21:07 - Syncaidius: if the update thread missed something
            21:07 - Syncaidius: it vanished
            21:07 - Syncaidius: xD
            21:07 - Zeracronius: which means that the FPS itself is purely GPU interaction
            21:07 - Zeracronius: nothing logic
            21:07 - Zeracronius: hmmm
            21:07 - Zeracronius: with that system
            21:08 - Zeracronius: frame timelocks would be feasable?
            21:08 - Zeracronius: though
            21:08 - Zeracronius: you'd get stuff jumping
            21:08 - Syncaidius: i almost had that working perfectly last time
            21:08 - Syncaidius: xD
            21:08 - Syncaidius: its just that design choice above fucked it all up
            21:08 - Syncaidius: i had time slicing where the schedular allocted frame time to a thread
            21:08 - Zeracronius: so the worst part is that the position it calculates to move to is only ready every 2nd frame
            21:08 - Zeracronius: so the object only moves every 2 frames
            21:08 - Syncaidius: if it exceeded it, it had to bail
            21:08 - Syncaidius: sorry
            21:08 - Syncaidius: halt
            21:08 - Syncaidius: not bail xD
            21:08 - Syncaidius: then it continues next frame
            21:08 - Syncaidius: or skips
            21:09 - Zeracronius: yeah
            21:09 - Syncaidius: depends what that system wants to do
            21:09 - Zeracronius: exactly
            21:09 - Syncaidius: tho i implemented a "skip if X frames behind"
            21:09 - Zeracronius: basicly
            21:09 - Zeracronius: Agile development
            21:09 - Zeracronius: sorry
            21:09 - Zeracronius: continous delivery*
            21:09 - Zeracronius: "release at predefined interval, whatever is ready at that time"
            21:09 - Syncaidius: 21:08 - Zeracronius: so the worst part is that the position it calculates to move to is only ready every 2nd frame
            21:08 - Zeracronius: so the object only moves every 2 frames
            21:09 - Syncaidius: that's where render interpolation comes in
            21:09 - Syncaidius: xD
            21:09 - Syncaidius: SE has that
            21:09 - Zeracronius: well
            21:09 - Syncaidius: + the option to turn it off
            21:10 - Zeracronius: yeah
            21:10 - Syncaidius: (because it adds a bit extra CPU load on the render thread)
            21:10 - Zeracronius: that's when stuff warps around at 9001 km/h
            21:10 - Zeracronius: trying to catch up
            21:10 - Syncaidius: they use the gpu to calculate it all
            21:10 - Syncaidius: so its not actually that bad
            21:10 - Syncaidius: unless ur GPU is a potato
            21:10 - Zeracronius: im talking from UX
            21:10 - Zeracronius: 22:09 - Syncaidius: that's where render interpolation comes in
            21:10 - Syncaidius: yeah
            21:10 - Zeracronius: since
            21:10 - Zeracronius: frame 1 where it should have moved
            21:10 - Zeracronius: doesn't know it needs to move
            21:10 - Zeracronius: frame 2 will show it moving from it's original position
            21:10 - Syncaidius: that's the neat part
            21:11 - Syncaidius: about keeping the render thread 3 - 4 frames behind
            21:11 - Zeracronius: very true
            21:11 - Syncaidius: u have some leeway on skipped frames on the update thread
            21:11 - Zeracronius: literally means
            21:11 - Syncaidius: because the render thread can go "hey, i didnt get data for X frame"
            21:11 - Syncaidius: and interpolate
            21:11 - Zeracronius: you can miss up to 4 frames
            21:11 - Zeracronius: and still interpolate
            21:11 - Syncaidius: yeah
            21:11 - Zeracronius: making it 100% smooth
            21:11 - Zeracronius: gawd
            21:11 - Zeracronius: it's complex
            21:11 - Zeracronius: but it'd work
            21:11 - Syncaidius: hey, you got it
            21:11 - Syncaidius: xD
            21:11 - Zeracronius: i think the hardest part about it is when you start MP
            21:12 - Syncaidius: stuff like that wont be easy at all
            21:12 - Syncaidius: but i did get animation interpolation to work
            21:12 - Zeracronius: each computer would be equally "behind"
            21:12 - Syncaidius: sooo
            21:12 - Syncaidius: i have a chance
            21:12 - Syncaidius: xD
        */
    }
}
