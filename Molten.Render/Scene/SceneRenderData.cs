using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public delegate void SceneRenderDataHandler(IRenderer renderer, SceneRenderData data);

    /// <summary>
    /// A class for storing renderer-specific information about a scene.
    /// </summary>
    public abstract class SceneRenderData : EngineObject
    {
        /// <summary>
        /// Occurs just before the scene is about to be rendered.
        /// </summary>
        public event SceneRenderDataHandler OnPreRender;

        /// <summary>
        /// Occurs just after the scene has been rendered.
        /// </summary>
        public event SceneRenderDataHandler OnPostRender;

        public LightList PointLights = new LightList(100, 100);

        public LightList CapsuleLights = new LightList(50, 100);

        /// <summary>
        /// If true, the scene will be rendered.
        /// </summary>
        public bool IsVisible = true;

        public bool Skip;

        /// <summary>The camera that should be used as a view or eye when rendering 3D objects in a scene.</summary>
        public ICamera Camera;

        public IRenderSurface FinalSurface;

        /// <summary>
        /// Flags which describe basic rules for rendering the scene.
        /// </summary>
        public SceneRenderFlags Flags = SceneRenderFlags.Render2D | SceneRenderFlags.Render3D;

        /// <summary>
        /// The background color of the scene.
        /// </summary>
        public Color BackgroundColor = new Color(20,20,20,255);

        /// <summary>
        /// The ambient light color.
        /// </summary>
        public Color AmbientLightColor = Color.Black;

        public Matrix4F View = Matrix4F.Identity;
        public Matrix4F Projection;
        public Matrix4F ViewProjection;
        public Matrix4F InvViewProjection;

        public readonly List<IRenderable2D> Renderables2D = new List<IRenderable2D>();
        public readonly RenderProfiler Profiler = new RenderProfiler();
        protected readonly ThreadedQueue<RenderSceneChange> _pendingChanges = new ThreadedQueue<RenderSceneChange>();
        ISceneDebugOverlay _overlay;

        public SceneRenderData(ISceneDebugOverlay overlay)
        {
            _overlay = overlay;
        }

        public void AddObject(IRenderable2D obj)
        {
            Add2D change = Add2D.Get();
            change.Object = obj;
            change.Data = this;
            _pendingChanges.Enqueue(change);
        }

        public void RemoveObject(IRenderable2D obj)
        {
            Remove2D change = Remove2D.Get();
            change.Object = obj;
            change.Data = this;
            _pendingChanges.Enqueue(change);
        }


        public abstract void AddObject(IRenderable3D obj, ObjectRenderData renderData);

        public abstract void RemoveObject(IRenderable3D obj, ObjectRenderData renderData);

        public void ProcessChanges()
        {
            while (_pendingChanges.TryDequeue(out RenderSceneChange change))
                change.Process();
        }

        /// <summary>
        /// Returns true if the current <see cref="SceneRenderData"/> has the specified flag(s).
        /// </summary>
        /// <param name="flags">The flags to check.</param>
        /// <returns></returns>
        public bool HasFlag(SceneRenderFlags flags)
        {
            return (Flags & flags) == flags;
        }

        /// <summary>
        /// Invokes <see cref="OnPreRender"/> event.
        /// </summary>
        public void PreRenderInvoke(IRenderer renderer) => OnPreRender?.Invoke(renderer, this);

        /// <summary>
        /// Invokes <see cref="OnPostRender"/> event.
        /// </summary>
        public void PostRenderInvoke(IRenderer renderer) => OnPostRender?.Invoke(renderer, this);

        /* TODO:
        *  - Edit PointLights and CapsuleLights.Data directly in light scene components (e.g. PointLightComponent).
        *  - Renderer will upload the latest data to the GPU 
        */

        /// <summary>
        /// GGets the debug overlay which displays information for the current scene.
        /// </summary>
        public ISceneDebugOverlay DebugOverlay => _overlay;
    }

    public class SceneRenderData<R> : SceneRenderData
        where R: class, IRenderable3D
    {
        public Dictionary<R, List<ObjectRenderData>> Renderables;

        public SceneRenderData(ISceneDebugOverlay overlay) : base(overlay)
        {
            Renderables = new Dictionary<R, List<ObjectRenderData>>();
        }

        public override void AddObject(IRenderable3D obj, ObjectRenderData renderData)
        {
            RenderableAdd<R> change = RenderableAdd<R>.Get();
            change.Renderable = obj as R;
            change.Data = renderData;
            change.SceneData = this;
            _pendingChanges.Enqueue(change);
        }

        public override void RemoveObject(IRenderable3D obj, ObjectRenderData renderData)
        {
            RenderableRemove<R> change = RenderableRemove<R>.Get();
            change.Renderable = obj as R;
            change.Data = renderData;
            change.SceneData = this;
            _pendingChanges.Enqueue(change);
        }
    }
}
