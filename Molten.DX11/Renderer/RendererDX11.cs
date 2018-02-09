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
        static readonly Matrix _defaultView2D = Matrix.Identity;
        static readonly Matrix _defaultView3D = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);

        DX11DisplayManager _displayManager;
        ResourceManager _resourceManager;
        MaterialManager _materials;
        ComputeManager _compute;
        GraphicsDevice _device;
        Logger _log;
        RenderProfilerDX _profiler;
        HlslCompiler _shaderCompiler;
        Material _defaultMaterial;
        ThreadedQueue<RendererTask> _tasks;
        ThreadedList<ISwapChainSurface> _outputSurfaces;
        HashSet<TextureAsset2D> _usedSurfaces;

        List<DebugOverlayPage> _debugOverlay;
        int _debugOverlayPage = 0;
        ISpriteFont _debugFont;
        bool _debugOverlayVisible = false;
        
        internal SpriteBatch SpriteBatcher;
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
        }

        public void InitializeAdapter(GraphicsSettings settings)
        {
            _displayManager = new DX11DisplayManager();
            _displayManager.Initialize(_log, settings);
        }

        public void InitializeRenderer(GraphicsSettings settings)
        {
            _profiler = new RenderProfilerDX();
            _outputSurfaces = new ThreadedList<ISwapChainSurface>();
            _device = new GraphicsDevice(_log, settings, _profiler, _displayManager, settings.EnableDebugLayer);
            _resourceManager = new ResourceManager(this);
            _materials = new MaterialManager();
            _compute = new ComputeManager(this.Device);
            _shaderCompiler = new HlslCompiler(this, _log);
            _tasks = new ThreadedQueue<RendererTask>();
            _usedSurfaces = new HashSet<TextureAsset2D>();
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
            SpriteBatcher = new SpriteBatch(this);

            InitializeDebugOverlay();
        }

        private void InitializeDebugOverlay()
        {
            _debugFont = _resourceManager.CreateFont("arial", 14);
            _debugOverlay = new List<DebugOverlayPage>();
            _debugOverlay.Add(new DebugStatsPage());
            _debugOverlay.Add(new DebugBuffersPage());
        }

        public int SetDebugOverlayPage(bool visible, int page)
        {
            if (page >= _debugOverlay.Count)
                page = _debugOverlay.Count - 1;

            int next = page + 1;
            if (next >= _debugOverlay.Count)
                next = 0;

            _debugOverlayPage = page;
            _debugOverlayVisible = visible;
            return next;
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

        public void Present(Timing time)
        {
            _profiler.StartCapture();
            _device.Profiler.StartCapture();

            // Perform all queued tasks before proceeding
            RendererTask task = null;
            while (_tasks.TryDequeue(out task))
                task.Process(this);

            //// Clear all output surfaces before rendering anything.
            //_outputSurfaces.ForInterlock(0, 1, (index, surface) =>
            //{
            //    surface.Clear(surface.PresentClearColor);
            //    return false;
            //});

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

            // TODO do renderer stuff here (i.e. render scenes, do deferred rendering, do post-processing, etc, etc).
            for (int i = 0; i < Scenes.Count; i++)
            {
                SceneRenderDataDX11 scene = Scenes[i];
                if (scene.IsVisible)
                {
                    scene.ApplyChanges(_device);
                    Render3D(scene);
                    Render2D(scene, time);
                }
            }

            // Present all output surfaces
            _outputSurfaces.ForInterlock(0, 1, (index, surface) =>
            {                
                surface.Present();
                return false;
            });

            // Clear the list of used surfaces, ready for the next frame.
            _usedSurfaces.Clear();

            _profiler.AddData(_device.Profiler.CurrentFrame);
            _device.Profiler.EndCapture(time);
            _profiler.EndCapture(time);
        }

        private void Render3D(SceneRenderDataDX11 scene)
        {
            RenderSurfaceBase rs = null;
            DepthSurface ds = null;

            if (scene.RenderCamera != null)
            {
                rs = scene.RenderCamera.OutputSurface as RenderSurfaceBase;
                ds = scene.RenderCamera.OutputDepthSurface as DepthSurface;
                rs = rs ?? _device.DefaultSurface;

                scene.Projection = scene.RenderCamera.Projection;
                scene.View = scene.RenderCamera.View;
                scene.ViewProjection = scene.RenderCamera.ViewProjection;
            }
            else
            {
                rs = _device.DefaultSurface;
                if (rs == null)
                    return;

                scene.View = _defaultView3D;
                scene.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, rs.Width / (float)rs.Height, 0.1f, 100.0f);
                scene.ViewProjection = Matrix.Multiply(scene.View, scene.Projection);
            }

            if (rs != null)
            {
                if (!_usedSurfaces.Contains(rs))
                {
                    rs.Clear(scene.BackgroundColor);
                    _usedSurfaces.Add(rs);
                }

                // Clear the depth surface if it hasn't already been cleared
                if (ds != null && !_usedSurfaces.Contains(ds))
                {
                    ds.Clear(DepthClearFlags.Depth | DepthClearFlags.Stencil);
                    _usedSurfaces.Add(ds);
                }

                _device.SetRenderSurface(rs, 0);
                _device.SetDepthSurface(ds, GraphicsDepthMode.Enabled);
                _device.DepthStencil.SetPreset(DepthStencilPreset.Default);
                _device.Rasterizer.SetViewports(rs.Viewport);
                scene.Render3D(_device, this);
            }
        }

        private void Render2D(SceneRenderDataDX11 scene, Timing time)
        {
            Matrix spriteView, spriteProj, spriteViewProj;
            RenderSurfaceBase rs = null;
            DepthSurface ds = null;

            if (scene.SpriteCamera != null)
            {
                rs = scene.SpriteCamera.OutputSurface as RenderSurfaceBase;
                ds = scene.SpriteCamera.OutputDepthSurface as DepthSurface;

                spriteProj = scene.SpriteCamera.Projection;
                spriteView = scene.RenderCamera.View;
                spriteViewProj = scene.SpriteCamera.ViewProjection;
            }
            else
            {
                rs = _device.DefaultSurface;
                if (rs == null)
                    return;

                spriteProj = _defaultView2D;
                spriteView = Matrix.OrthoOffCenterLH(0, rs.Width, -rs.Height, 0, 0, 1);
                spriteViewProj = Matrix.Multiply(spriteView, spriteProj);
            }

            if (rs != null)
            {
                if (!_usedSurfaces.Contains(rs))
                {
                    rs.Clear(scene.BackgroundColor);
                    _usedSurfaces.Add(rs);
                }

                // Clear the depth surface if it hasn't already been cleared
                if (ds != null && !_usedSurfaces.Contains(ds))
                {
                    ds.Clear(DepthClearFlags.Depth | DepthClearFlags.Stencil);
                    _usedSurfaces.Add(ds);
                }

                _device.SetRenderSurface(rs, 0);
                _device.SetDepthSurface(ds, GraphicsDepthMode.Enabled);
                _device.DepthStencil.SetPreset(DepthStencilPreset.Default);
                _device.Rasterizer.SetViewports(rs.Viewport);

                SpriteBatcher.Begin(rs.Viewport);
                scene.Render2D(_device, this);

                // Render the debug overlay here so it shows on top of everything else
                if (_debugOverlayVisible)
                    _debugOverlay[_debugOverlayPage].Render(_debugFont, this, SpriteBatcher, time, rs);

                SpriteBatcher.Flush(_device, ref spriteViewProj, BlendingPreset.PreMultipliedAlpha);
            }
        }

        public void Dispose()
        {
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

        internal Material DefaultMaterial => _defaultMaterial;

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
