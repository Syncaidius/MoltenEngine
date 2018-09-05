using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// A base class that custom renderer implementations must inherit in order to be compatible with Molten engine, as it provides basic functionality for interacting with the rest of the engine.
    /// </summary>
    public abstract class RenderEngine : IDisposable
    {
        public static readonly Matrix4F DefaultView3D = Matrix4F.LookAtLH(new Vector3F(0, 0, -5), new Vector3F(0, 0, 0), Vector3F.UnitY);

        bool _surfaceResizeRequired;
        AntiAliasMode _requestedMultiSampleLevel = AntiAliasMode.None;
        internal AntiAliasMode MsaaLevel = AntiAliasMode.None;

        /// <summary>
        /// Creates a new instance of a <see cref="RenderEngine"/> sub-type.
        /// </summary>
        public RenderEngine()
        {
            Log = Logger.Get();
            Log.AddOutput(new LogFileWriter($"renderer_{Name.Replace(' ', '_')}{0}.txt"));
        }

        public void InitializeAdapter(GraphicsSettings settings)
        {
            OnInitializeAdapter(settings);
        }

        public void Initialize(GraphicsSettings settings)
        {
            settings.Log(Log, "Graphics");
            MsaaLevel = _requestedMultiSampleLevel = MsaaLevel;
            settings.MSAA.OnChanged += MSAA_OnChanged;

            OnInitialize(settings);
        }

        /// <summary>
        /// Occurs when a graphics adapter needs to be acquired, before the renderer is initialized.
        /// </summary>
        /// <param name="settings"></param>
        protected abstract void OnInitializeAdapter(GraphicsSettings settings);

        /// <summary>
        /// Occurs when the renderer is being initialized.
        /// </summary>
        /// <param name="settings"></param>
        protected abstract void OnInitialize(GraphicsSettings settings);

        private void MSAA_OnChanged(AntiAliasMode oldValue, AntiAliasMode newValue)
        {
            _requestedMultiSampleLevel = newValue;
        }

        public SceneRenderData CreateRenderData()
        {
            SceneRenderData rd = OnCreateRenderData();
            RendererAddScene task = RendererAddScene.Get();
            task.Data = rd;
            PushTask(task);
            return rd;
        }

        protected abstract SceneRenderData OnCreateRenderData();

        public void DestroyRenderData(SceneRenderData data)
        {
            RendererRemoveScene task = RendererRemoveScene.Get();
            task.Data = data as SceneRenderData;
            PushTask(task);
        }

        private void PushSceneReorder(SceneRenderData data, SceneReorderMode mode)
        {
            RendererReorderScene task = RendererReorderScene.Get();
            task.Data = data as SceneRenderData;
            task.Mode = mode;
            PushTask(task);
        }

        public void BringToFront(SceneRenderData data)
        {
            PushSceneReorder(data, SceneReorderMode.BringToFront);
        }

        public void SendToBack(SceneRenderData data)
        {
            PushSceneReorder(data, SceneReorderMode.SendToBack);
        }

        public void PushForward(SceneRenderData data)
        {
            PushSceneReorder(data, SceneReorderMode.PushForward);
        }

        public void PushBackward(SceneRenderData data)
        {
            PushSceneReorder(data, SceneReorderMode.PushBackward);
        }

        public void PushTask(RendererTask task)
        {
            Tasks.Enqueue(task);
        }

        public void Present(Timing time)
        {
            Profiler.StartCapture();
            OnPrePresent(time);

            if (_requestedMultiSampleLevel != MsaaLevel)
            {
                // TODO re-create all internal surfaces/textures to match the new sample level.
                // TODO adjust rasterizer mode accordingly (multisample enabled/disabled).
                MsaaLevel = _requestedMultiSampleLevel;
                _surfaceResizeRequired = true;
            }

            // Perform all queued tasks before proceeding
            RendererTask task = null;
            while (Tasks.TryDequeue(out task))
                task.Process(this);

            // Perform preliminary checks on active scene data.
            // Also ensure the backbuffer is always big enough for the largest scene render surface.
            foreach (SceneRenderData data in Scenes)
            {
                data.Skip = false;

                if (!data.IsVisible || data.Camera == null)
                {
                    data.Skip = true;
                    continue;
                }

                // Check for valid final surface.
                data.FinalSurface = data.Camera.OutputSurface ?? DefaultSurface;
                if (data.FinalSurface == null)
                {
                    data.Skip = true;
                    continue;
                }

                if (data.FinalSurface.Width > BiggestWidth)
                {
                    _surfaceResizeRequired = true;
                    BiggestWidth = data.FinalSurface.Width;
                }

                if (data.FinalSurface.Height > BiggestHeight)
                {
                    _surfaceResizeRequired = true;
                    BiggestHeight = data.FinalSurface.Height;
                }
            }

            // Update surfaces if dirty. This may involve resizing or changing their format.
            if (_surfaceResizeRequired)
            {
                OnRebuildSurfaces(BiggestWidth, BiggestHeight);
                _surfaceResizeRequired = false;
            }

            OnPresent(time);

            // Present all output surfaces
            OutputSurfaces.ForInterlock(0, 1, (index, surface) =>
            {
                surface.Present();
                return false;
            });

            // Clear references to final surfaces. 
            // This is done separately so that any debug overlays rendered by scenes can still access final surface information during their render call.
            for (int i = 0; i < Scenes.Count; i++)
                Scenes[i].FinalSurface = null;

            Profiler.EndCapture(time);
            OnPostPresent(time);
        }

        protected abstract void OnRebuildSurfaces(int requiredWidth, int requiredHeight);
        protected abstract void OnPrePresent(Timing time);
        protected abstract void OnPresent(Timing time);

        /// <summary>
        /// Occurs after render presentation is completed and profiler timing has been finalized for the current frame. Useful if you need to do some per-frame cleanup/resetting.
        /// </summary>
        /// <param name="time"></param>
        protected abstract void OnPostPresent(Timing time);

        public void Dispose()
        {
            OutputSurfaces.ForInterlock(0, 1, (index, surface) =>
            {
                surface.Dispose();
                return false;
            });

            OnDispose();
            Log.Dispose();
        }

        /// <summary>
        /// Occurs when the current <see cref="RenderEngine"/> instance/implementation is being disposed.
        /// </summary>
        protected abstract void OnDispose();

        /// <summary>
        /// Gets profiling data attached to the renderer.
        /// </summary>
        public RenderProfiler Profiler { get; } = new RenderProfiler();

        /// <summary>
        /// Gets the display manager bound to the renderer.
        /// </summary>
        public abstract IDisplayManager DisplayManager { get; }

        /// <summary>
        /// Gets the resource manager bound to the renderer.
        /// This is responsible for creating and destroying graphics resources, such as buffers, textures and surfaces.
        /// </summary>
        public abstract IResourceManager Resources { get; }

        public abstract IRenderSurface DefaultSurface { get; set; }

        public abstract IComputeManager Compute { get; }

        /// <summary>
        /// Gets the name of the renderer implementation.
        /// </summary>
        public abstract string Name { get; }

        public ThreadedList<ISwapChainSurface> OutputSurfaces { get; } = new ThreadedList<ISwapChainSurface>();
        protected internal List<SceneRenderData> Scenes { get; }  = new List<SceneRenderData>();

        private ThreadedQueue<RendererTask> Tasks { get; } = new ThreadedQueue<RendererTask>();

        protected int BiggestWidth { get; private set; } = 1;

        protected int BiggestHeight { get; private set; } = 1;

        protected internal Logger Log { get; }
    }
}
