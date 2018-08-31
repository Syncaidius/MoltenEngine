using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>An object for storing render data about a <see cref="Scene"/>.</summary>
    public sealed class SceneRenderDataDX11 : SceneRenderData
    {        
        SceneRenderFlags _previousFlags;

        internal Dictionary<Renderable, List<ObjectRenderData>> Renderables;
        internal RenderSurfaceBase FinalSurface;

        internal bool Skip;

        SceneDebugOverlay _overlay;

        internal SceneRenderDataDX11(RendererDX11 renderer)
        {
            Renderables = new Dictionary<Renderable, List<ObjectRenderData>>();
            _previousFlags = Flags;
            _overlay = new SceneDebugOverlay(renderer, this);
        }

        public override void AddObject(IRenderable3D obj, ObjectRenderData renderData)
        {
            RenderableAdd change = RenderableAdd.Get();
            change.Renderable = obj as Renderable;
            change.Data = renderData;
            change.SceneData = this;
            _pendingChanges.Enqueue(change);
        }

        public override void RemoveObject(IRenderable3D obj, ObjectRenderData renderData)
        {
            RenderableRemove change = RenderableRemove.Get();
            change.Renderable = obj as Renderable;
            change.Data = renderData;
            change.SceneData = this;
            _pendingChanges.Enqueue(change);
        }

        /// <summary>
        /// Gets the scene's debug overlay.
        /// </summary>
        public override ISceneDebugOverlay DebugOverlay => _overlay;
    }
}
