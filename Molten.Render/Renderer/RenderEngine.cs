using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// A base class that custom renderer implementations must inherit. Provides basic functionality for interacting with the rest of the engine.
    /// </summary>
    public abstract class RenderEngine : IDisposable
    {
        public static readonly Matrix4F DefaultView3D = Matrix4F.LookAtLH(new Vector3F(0, 0, -5), new Vector3F(0, 0, 0), Vector3F.UnitY);

        public abstract void InitializeAdapter(GraphicsSettings settings);

        public abstract void Initialize(GraphicsSettings settings);

        protected internal List<SceneRenderData> Scenes = new List<SceneRenderData>();

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

        protected void ProcessPendingTasks()
        {
            // Perform all queued tasks before proceeding
            RendererTask task = null;
            while (Tasks.TryDequeue(out task))
                task.Process(this);
        }

        public void Present(Timing time)
        {
            Profiler.StartCapture();
            OnPresent(time);
            Profiler.EndCapture(time);
        }

        protected abstract void OnPresent(Timing time);

        public abstract void Dispose();

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

        public abstract ThreadedList<ISwapChainSurface> OutputSurfaces { get; }

        private ThreadedQueue<RendererTask> Tasks { get; } = new ThreadedQueue<RendererTask>();
    }
}
