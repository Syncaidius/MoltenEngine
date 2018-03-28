using SharpDX.Direct3D11;
using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class RendererDX11 : IRenderer
    {
        int _biggestWidth = 1;
        int _biggestHeight = 1;
        bool _surfacesDirty;

        DX11DisplayManager _displayManager;
        ResourceManager _resourceManager;
        MaterialManager _materials;
        ComputeManager _compute;
        GraphicsDevice _device;
        Logger _log;
        RenderProfilerDX _profiler;
        HlslCompiler _shaderCompiler;
        ThreadedQueue<RendererTask> _tasks;
        ThreadedList<ISwapChainSurface> _outputSurfaces;
        HashSet<TextureAsset2D> _clearedSurfaces;
        Material _defaultMeshMaterial;

        List<DebugOverlayPage> _debugOverlay;
        int _debugOverlayPage = 0;
        SpriteFont _debugFont;
        bool _debugOverlayVisible = false;
        Dictionary<Type, DeferredRenderStep> _steps;
        List<DeferredRenderStep> _stepList;

        // Core deferred renderer steps
        RenderChain _mainChain;

        AntiAliasMode _requestedMultiSampleLevel = AntiAliasMode.None;
        internal AntiAliasMode MsaaLevel = AntiAliasMode.None;
        internal SpriteBatchDX11 SpriteBatcher;
        internal List<SceneRenderDataDX11> Scenes;
        internal GraphicsBuffer StaticVertexBuffer;
        internal GraphicsBuffer StaticIndexBuffer;
        internal GraphicsBuffer DynamicVertexBuffer;
        internal GraphicsBuffer DynamicIndexBuffer;
        internal StagingBuffer StagingBuffer;

        public RendererDX11()
        {
            _log = Logger.Get();
            _log.AddOutput(new LogFileWriter("renderer_dx11{0}.txt"));
            _steps = new Dictionary<Type, DeferredRenderStep>();
            _stepList = new List<DeferredRenderStep>();
        }

        public void InitializeAdapter(GraphicsSettings settings)
        {
            _displayManager = new DX11DisplayManager();
            _displayManager.Initialize(_log, settings);
        }

        public void InitializeRenderer(GraphicsSettings settings)
        {
            settings.Log(_log, "Graphics");
            MsaaLevel = 
            _requestedMultiSampleLevel = MsaaLevel;
            settings.MSAA.OnChanged += MSAA_OnChanged;

            _profiler = new RenderProfilerDX();
            _outputSurfaces = new ThreadedList<ISwapChainSurface>();
            _device = new GraphicsDevice(_log, settings, _profiler, _displayManager, settings.EnableDebugLayer);
            _resourceManager = new ResourceManager(this);
            _materials = new MaterialManager();
            _compute = new ComputeManager(this.Device);
            _shaderCompiler = new HlslCompiler(this, _log);
            _tasks = new ThreadedQueue<RendererTask>();
            _clearedSurfaces = new HashSet<TextureAsset2D>();
            _mainChain = new RenderChain(this);
            Scenes = new List<SceneRenderDataDX11>();

            int maxVertexBytesStatic = 1024 * 512;
            int maxIndexBytesStatic = 1024 * 300;
            StaticVertexBuffer = new GraphicsBuffer(_device, BufferMode.Default, BindFlags.VertexBuffer, maxVertexBytesStatic);
            StaticIndexBuffer = new GraphicsBuffer(_device, BufferMode.Default, BindFlags.IndexBuffer, maxIndexBytesStatic);

            int maxVertexBytesDynamic = 1024 * 512;
            int maxIndexBytesDynamic = 1024 * 300;
            DynamicVertexBuffer = new GraphicsBuffer(_device, BufferMode.Dynamic, BindFlags.VertexBuffer, maxVertexBytesDynamic);
            DynamicIndexBuffer = new GraphicsBuffer(_device, BufferMode.Dynamic, BindFlags.IndexBuffer, maxIndexBytesDynamic);

            StagingBuffer = new StagingBuffer(_device, StagingBufferFlags.Write, maxVertexBytesStatic / 4);
            SpriteBatcher = new SpriteBatchDX11(this, 3000);
             
            /* TODO: 
             *  - Allow the renderer to iterate over sprite layers instead of inside SceneData.Render2D().
             *  - Remove 2D layer system and add render flags to SceneData to control how/where 2D is rendered (e.g. behind post-processing, ahead (default) or behind 3D).
             *  - Add normal map support to GBUFFER
             *  - Add standard texture properties to IMesh (Albedo, normal, specular, emissive, PBR, etc)
             *  - Stages:
             *      -- GBUFFER - WIP
             *      -- lighting
             *      -- emssive
             *      -- HDR/Bloom
             *      -- Tone mapping
             *      -- Output/finalize (render 2D here)
             *      
             *  
             *  - iterate over _stepList for every active scene. 
             */

            InitializeDebugOverlay();
        }

        private void InitializeDebugOverlay()
        {
            _debugOverlay = new List<DebugOverlayPage>();
            _debugOverlay.Add(new DebugStatsPage());
            _debugOverlay.Add(new DebugBuffersPage());
        }

        public int SetDebugOverlayPage(SpriteFont font, bool visible, int page)
        {
            _debugFont = font;

            if (page >= _debugOverlay.Count)
                page = _debugOverlay.Count - 1;

            int next = page + 1;
            if (next >= _debugOverlay.Count)
                next = 0;

            _debugOverlayPage = page;
            _debugOverlayVisible = visible;
            return next;
        }

        internal void DrawDebugOverlay(SpriteBatch sb, SceneRenderDataDX11 scene, Timing time, IRenderSurface rs)
        {
            // Render the debug overlay here so it shows on top of everything else
            if (_debugOverlayVisible && !scene.HasFlag(SceneRenderFlags.NoDebugOverlay))
                _debugOverlay[_debugOverlayPage].Render(_debugFont, this, SpriteBatcher, time, rs);
        }

        public void DispatchCompute(IComputeTask task, int x, int y, int z)
        {
            _device.ExternalContext.Dispatch(task as ComputeTask, x, y, z);
        }

        public SceneRenderData CreateRenderData()
        {
            SceneRenderDataDX11 rd = new SceneRenderDataDX11();
            RendererAddScene task = RendererAddScene.Get();
            task.Data = rd;
            PushTask(task);
            return rd;
        }

        public void DestroyRenderData(SceneRenderData data)
        {
            SceneRenderDataDX11 rd = data as SceneRenderDataDX11;
            RendererRemoveScene task = RendererRemoveScene.Get();
            task.Data = rd;
            PushTask(task);
        }

        internal void PushTask(RendererTask task)
        {
            _tasks.Enqueue(task);
        }

        internal T GetRenderStep<T>() where T : DeferredRenderStep, new()
        {
            Type t = typeof(T);
            DeferredRenderStep step;
            if (!_steps.TryGetValue(t, out step))
            {
                step = new T();
                step.Initialize(this, _biggestWidth, _biggestHeight);
                _steps.Add(t, step);
                _stepList.Add(step);
            }

            return step as T;
        }

        private void SetNextStep<T, NEXT>() where T : DeferredRenderStep, new()
            where NEXT : DeferredRenderStep, new()
        {
            if(_steps.TryGetValue(typeof(T), out DeferredRenderStep step))
                step.Next = GetRenderStep<NEXT>();
        }

        private void SetNoNextStep<T>() where T : DeferredRenderStep, new()
        {
            if (_steps.TryGetValue(typeof(T), out DeferredRenderStep step))
                step.Next = null;
        }

        public void Present(Timing time)
        {
            _profiler.StartCapture();
            _device.Profiler.StartCapture();

            if(_requestedMultiSampleLevel != MsaaLevel)
            {
                // TODO re-create all internal surfaces/textures to match the new sample level.
                // TODO adjust rasterizer mode accordingly (multisample enabled/disabled).
                MsaaLevel = _requestedMultiSampleLevel;
                _surfacesDirty = true;
            }

            // Perform all queued tasks before proceeding
            RendererTask task = null;
            while (_tasks.TryDequeue(out task))
                task.Process(this);

            // Ensure the backbuffer is always big enough for the largest scene render surface.
            foreach (SceneRenderDataDX11 data in Scenes)
            {
                RenderSurfaceBase rs = _device.DefaultSurface;
                if (data.RenderCamera != null)
                    rs = data.RenderCamera.OutputSurface as RenderSurfaceBase ?? rs;

                if (rs == null)
                    continue;

                // Cache the surface we'll be using to render the scene data.
                data.ChosenSurface = rs;

                if (rs.Width > _biggestWidth)
                {
                    _surfacesDirty = true;
                    _biggestWidth = rs.Width;
                }

                if (rs.Height > _biggestHeight)
                {
                    _surfacesDirty = true;
                    _biggestHeight = rs.Height;
                }
            }

            // Update surfaces if dirty. This may involve resizing or changing their format.
            if (_surfacesDirty)
            {
                for (int i = 0; i < _stepList.Count; i++)
                    _stepList[i].UpdateSurfaces(this, _biggestWidth, _biggestHeight);

                _surfacesDirty = false;
            }

            /* DESIGN NOTES:
             *  - Store a hashset of materials used in each scene so that the renderer can set the "Common" buffer in one pass
             *  
             *  
             * MULTI-THREADING
             *  - Consider using 2+ worker threads to prepare a command list/deferred context from each scene, which can then
             *    be dispatched to the immediate context when all scenes have been processed
             *  - Avoid the above if any scenes interact with a render form surface at any point, since those can only be handled on the thread they're created on.
             *  
             *  - Consider using worker threads to:
             *      -- Sort front-to-back for rendering opaque objects (front-to-back reduces overdraw)
             *      -- Sort by buffer, material or textures (later in time)
             *      -- Sort back-to-front for rendering transparent objects (back-to-front reduces issues in alpha-blending)
             *  
             * 
             * 2D & UI Rendering:
             *  - Provide a sprite-batch for rendering 2D and UI
             *  - Prepare rendering of these on worker threads.
             */

            SceneRenderDataDX11 scene;
            for (int i = 0; i < Scenes.Count; i++)
            {
                scene = Scenes[i];
                if (scene.IsVisible)
                {
                    BuildChain(scene, _mainChain);
                    _mainChain.Render(this, scene, time);
                }
            }

            // Present all output surfaces
            _outputSurfaces.ForInterlock(0, 1, (index, surface) =>
            {                
                surface.Present();
                return false;
            });

            // Clear the list of used surfaces, ready for the next frame.
            _clearedSurfaces.Clear();

            _profiler.AddData(_device.Profiler.CurrentFrame);
            _device.Profiler.EndCapture(time);
            _profiler.EndCapture(time);
        }

        internal void ClearIfFirstUse(TextureAsset2D surface, Action callback)
        {
            if(!_clearedSurfaces.Contains(surface))
            {
                callback();
                _clearedSurfaces.Add(surface);
            }
        }

        private void BuildChain(SceneRenderDataDX11 scene, RenderChain chain)
        {
            // TODO if the current scene has the same flags as the previous scene, skip rebuilding chain.
            // TODO consider moving/caching render chain construction in to SceneRenderDataDX11. If the flags are changed, (re)build it on the next render cycle.

            chain.Clear();

            if (scene.HasFlag(SceneRenderFlags.Deferred))
            {
                chain.Next<GBuffer3dStep>();
                // TODO complete deferred chain here
            }
            else
            {
                if (scene.HasFlag(SceneRenderFlags.Render3D))
                    chain.Next<Immediate3dStep>();

                if (scene.HasFlag(SceneRenderFlags.Render2D))
                    chain.Next<Render2dStep>();
            }
        }

        private void MSAA_OnChanged(AntiAliasMode oldValue, AntiAliasMode newValue)
        {
            _requestedMultiSampleLevel = newValue;
        }

        public void Dispose()
        {
            for (int i = 0; i < _stepList.Count; i++)
                _stepList[i].Dispose();

            _outputSurfaces.ForInterlock(0, 1, (index, surface) =>
            {
                surface.Dispose();
                return false;
            });

            _resourceManager.Dispose();
            _device?.Dispose();
            _displayManager?.Dispose();
            _log.Dispose();
            SpriteBatcher.Dispose();

            StaticVertexBuffer.Dispose();
            StaticIndexBuffer.Dispose();

            DynamicVertexBuffer.Dispose();
            DynamicIndexBuffer.Dispose();
        }

        /// <summary>
        /// Gets the name of the renderer.
        /// </summary>
        public string Name => "DirectX 11";

        /// <summary>
        /// Gets the display manager bound to the renderer.
        /// </summary>
        public IDisplayManager DisplayManager => _displayManager;

        /// <summary>
        /// Gets profiling data attached to the renderer.
        /// </summary>
        public IRenderProfiler Profiler => _profiler;

        internal GraphicsDevice Device => _device;

        internal MaterialManager Materials => _materials;

        IMaterialManager IRenderer.Materials => _materials;

        internal ComputeManager Compute => _compute;

        IComputeManager IRenderer.Compute => _compute;

        internal Material DefaultMeshMaterial => _defaultMeshMaterial;

        internal HlslCompiler ShaderCompiler => _shaderCompiler;

        /// <summary>
        /// Gets the resource manager bound to the renderer.
        /// This is responsible for creating and destroying graphics resources, such as buffers, textures and surfaces.
        /// </summary>
        public IResourceManager Resources => _resourceManager;

        public ThreadedList<ISwapChainSurface> OutputSurfaces => _outputSurfaces;

        public IRenderSurface DefaultSurface
        {
            get => _device.DefaultSurface;
            set => _device.DefaultSurface = value as RenderSurfaceBase;
        }
    }
}
