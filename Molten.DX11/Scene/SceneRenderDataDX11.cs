using Molten.Collections;
using Molten.Graphics.Scene.Lights;
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
        ThreadedQueue<RenderSceneChange> _pendingChanges;
        RenderChain _chain;
        SceneRenderFlags _previousFlags;

        internal Dictionary<Renderable, List<ObjectRenderData>> Renderables;
        internal List<ISprite> Sprites;
        internal Matrix4F View = Matrix4F.Identity;
        internal Matrix4F Projection;
        internal Matrix4F ViewProjection;
        internal RenderSurfaceBase FinalSurface;
        internal RenderProfilerDX Profiler;

        SceneDebugOverlay _overlay;

        internal SceneRenderDataDX11(RendererDX11 renderer)
        {
            _pendingChanges = new ThreadedQueue<RenderSceneChange>();
            Renderables = new Dictionary<Renderable, List<ObjectRenderData>>();

            // Set up one sprite layer.
            Sprites = new List<ISprite>();
            _previousFlags = Flags;
            _chain = new RenderChain(renderer, this);
            _chain.Rebuild();
            Profiler = new RenderProfilerDX();
            _overlay = new SceneDebugOverlay(renderer, this);
        }

        public override void AddObject(IRenderable obj, ObjectRenderData renderData)
        {
            RenderableAdd change = RenderableAdd.Get();
            change.Renderable = obj as Renderable;
            change.Data = renderData;
            _pendingChanges.Enqueue(change);
        }

        public override void RemoveObject(IRenderable obj, ObjectRenderData renderData)
        {
            RenderableRemove change = RenderableRemove.Get();
            change.Renderable = obj as Renderable;
            change.Data = renderData;
            _pendingChanges.Enqueue(change);
        }

        public override void AddSprite(ISprite sprite)
        {
            SpriteAdd change = SpriteAdd.Get();
            change.Sprite = sprite;
            _pendingChanges.Enqueue(change);
        }

        public override void RemoveSprite(ISprite sprite)
        {
            SpriteRemove change = SpriteRemove.Get();
            change.Sprite = sprite;
            _pendingChanges.Enqueue(change);
        }

        internal void Render(GraphicsPipe pipe, RendererDX11 renderer, Timing time)
        {
            PreRenderInvoke(renderer);
            if (Flags != _previousFlags)
            {
                _chain.Rebuild();
                _previousFlags = Flags;
            }

            while (_pendingChanges.TryDequeue(out RenderSceneChange change))
                change.Process(this);

            _chain.Render(this, time);
            PostRenderInvoke(renderer);
        }

        internal void Render3D(GraphicsPipe pipe, RendererDX11 renderer)
        {
            // To start with we're just going to draw ALL objects in the render tree.
            // Sorting and culling will come later
            foreach (KeyValuePair<Renderable, List<ObjectRenderData>> p in Renderables)
            {
                foreach(ObjectRenderData data in p.Value)
                {
                    // TODO replace below with render prediction to interpolate between the current and target transform.
                    data.RenderTransform = data.TargetTransform;
                    p.Key.Render(pipe, renderer, data, this);
                }
            }

            /* TODO: 
             *  Procedure:
             *      1) Calculate:
             *          a) Capture the current transform matrix of each object in the render tree
             *          b) Calculate the distance from the scene camera. Store on RenderData
             *          
             *      2) Sort objects by distance from camera:
             *          a) Sort objects into buckets inside RenderTree, front-to-back (reduce overdraw by drawing closest first).
             *          b) Only re-sort a bucket when objects are added or the camera moves
             *          c) While sorting, build up separate bucket list of objects with a transparent material sorted back-to-front (for alpha to work)
             *          
             *  Extras:
             *      3) Reduce z-sorting needed in (2) by adding scene-graph culling (quad-tree, octree or BSP) later down the line.
             *      4) Reduce (3) further by frustum culling the graph-culling results
             * 
             *  NOTES:
                    - when SceneObject.IsVisible is changed, queue an Add or Remove operation on the RenderTree depending on visibility. This will remove it from culling/sorting.
             */
        }

        internal void Render2D(GraphicsPipe pipe, RendererDX11 renderer)
        {
            for(int i = 0; i < Sprites.Count; i++)
                Sprites[i].Render(renderer.SpriteBatcher);
        }

        /// <summary>
        /// Gets the scene's debug overlay.
        /// </summary>
        public override ISceneDebugOverlay DebugOverlay => _overlay;
    }
}
