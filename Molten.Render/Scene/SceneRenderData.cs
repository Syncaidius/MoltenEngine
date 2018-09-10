using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public delegate void SceneRenderDataHandler(RenderEngine renderer, SceneRenderData data);

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

        /// <summary>
        /// If true, the scene will be rendered.
        /// </summary>
        public bool IsVisible = true;

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

        public List<SceneLayerData> Layers = new List<SceneLayerData>();
        protected readonly ThreadedQueue<RenderSceneChange> _pendingChanges = new ThreadedQueue<RenderSceneChange>();
        ISceneDebugOverlay _overlay;

        public abstract SceneLayerData CreateLayerData();

        public void AddLayer(SceneLayerData data)
        {
            RenderLayerAdd change = RenderLayerAdd.Get();
            change.LayerData = data;
            change.SceneData = this;
            _pendingChanges.Enqueue(change);
        }

        public void RemoveLayer(SceneLayerData data)
        {
            RenderLayerRemove change = RenderLayerRemove.Get();
            change.LayerData = data;
            change.SceneData = this;
            _pendingChanges.Enqueue(change);
        }

        public void AddObject(IRenderable2D obj, SceneLayerData layerData)
        {
            Add2D change = Add2D.Get();
            change.Object = obj;
            change.LayerData = layerData;
            _pendingChanges.Enqueue(change);
        }

        public void RemoveObject(IRenderable2D obj, SceneLayerData layerData)
        {
            Remove2D change = Remove2D.Get();
            change.Object = obj;
            change.Layerdata = layerData;
            _pendingChanges.Enqueue(change);
        }

        public void AddObject(RenderCamera obj)
        {
            AddCamera change = AddCamera.Get();
            change.Camera = obj;
            change.Data = this;
            _pendingChanges.Enqueue(change);
        }

        public void RemoveObject(RenderCamera obj)
        {
            RemoveCamera change = RemoveCamera.Get();
            change.Camera = obj;
            change.Data = this;
            _pendingChanges.Enqueue(change);
        }

        public abstract void AddObject(IRenderable3D obj, ObjectRenderData renderData, SceneLayerData layer);

        public abstract void RemoveObject(IRenderable3D obj, ObjectRenderData renderData, SceneLayerData layer);

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
        public void PreRenderInvoke(RenderEngine renderer) => OnPreRender?.Invoke(renderer, this);

        /// <summary>
        /// Invokes <see cref="OnPostRender"/> event.
        /// </summary>
        public void PostRenderInvoke(RenderEngine renderer) => OnPostRender?.Invoke(renderer, this);

        /* TODO:
        *  - Edit PointLights and CapsuleLights.Data directly in light scene components (e.g. PointLightComponent).
        *  - Renderer will upload the latest data to the GPU 
        */

        /// <summary>
        /// GGets the debug overlay which displays information for the current scene.
        /// </summary>
        public ISceneDebugOverlay DebugOverlay { get; set; }

        /// <summary>
        /// Gets the <see cref="RenderProfiler"/> instance bound to the scene data. This tracks render performance and statistics for the current set of scene data.
        /// </summary>
        public RenderProfiler Profiler { get; } = new RenderProfiler();

        public LightList PointLights { get; } = new LightList(100, 100);

        public LightList CapsuleLights { get; } = new LightList(50, 100);

        public List<RenderCamera> Cameras { get; } = new List<RenderCamera>();
    }

    public class SceneRenderData<R> : SceneRenderData
        where R: class, IRenderable3D
    {
        public override SceneLayerData CreateLayerData()
        {
            return new SceneLayerData<R>();
        }

        public override void AddObject(IRenderable3D obj, ObjectRenderData renderData, SceneLayerData layer)
        {
            RenderableAdd<R> change = RenderableAdd<R>.Get();
            change.Renderable = obj as R;
            change.Data = renderData;
            change.LayerData = layer as SceneLayerData<R>;
            _pendingChanges.Enqueue(change);
        }

        public override void RemoveObject(IRenderable3D obj, ObjectRenderData renderData, SceneLayerData layer)
        {
            RenderableRemove<R> change = RenderableRemove<R>.Get();
            change.Renderable = obj as R;
            change.Data = renderData;
            change.LayerData = layer as SceneLayerData<R>;
            _pendingChanges.Enqueue(change);
        }
    }
}
