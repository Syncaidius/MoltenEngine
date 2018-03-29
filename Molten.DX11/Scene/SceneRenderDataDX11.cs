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
        ThreadedQueue<RenderSceneChange> _pendingChanges;
        RenderChain _chain;
        SceneRenderFlags _previousFlags;

        internal Dictionary<Renderable, List<ObjectRenderData>> Renderables;
        internal SpriteLayer[] SpriteLayers;


        internal SceneRenderDataDX11(RendererDX11 renderer)
        {
            _pendingChanges = new ThreadedQueue<RenderSceneChange>();
            Renderables = new Dictionary<Renderable, List<ObjectRenderData>>();

            // Set up one sprite layer.
            SpriteLayers = new SpriteLayer[1];
            SpriteLayers[0] = new SpriteLayer();
            _previousFlags = Flags;
            _chain = new RenderChain(renderer, this);
            _chain.Rebuild();
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

        public override void AddSprite(ISprite sprite, int layer = 0)
        {
            SpriteAdd change = SpriteAdd.Get();
            change.Sprite = sprite;
            change.Layer = layer;
            _pendingChanges.Enqueue(change);
        }

        public override void RemoveSprite(ISprite sprite, int layer = 0)
        {
            SpriteRemove change = SpriteRemove.Get();
            change.Sprite = sprite;
            change.Layer = layer;
            _pendingChanges.Enqueue(change);
        }

        public override void ClearSpriteLayer(int layer)
        {
            if (layer < SpriteLayers.Length)
                SpriteLayers[layer].Sprites.Clear();
        }

        public override void SetSpriteLayerVisibility(int layer, bool visible)
        {
            SpriteSetLayerVisibility change = SpriteSetLayerVisibility.Get();
            change.LayerID = layer;
            change.Visibility = visible;
            _pendingChanges.Enqueue(change);
        }

        public override void GetVisibleSpriteLayers(List<int> output, Action<List<int>> retrievalCallback)
        {
            SpriteGetVisibleLayers change = SpriteGetVisibleLayers.Get();
            change.Output = output;
            change.RetrievalCallback = retrievalCallback;
            _pendingChanges.Enqueue(change);
        }

        public override void SetSpriteLayerCount(int layerCount)
        {
            SpriteSetLayerCount change = SpriteSetLayerCount.Get();
            change.LayerCount = layerCount;
            _pendingChanges.Enqueue(change);
        }

        public override void ChangeSpriteLayer(ISprite sprite, int oldLayer, int newLayer)
        {
            SpriteChangeLayer change = SpriteChangeLayer.Get();
            change.Sprite = sprite;
            change.OldLayer = oldLayer;
            change.NewLayer = newLayer;
            _pendingChanges.Enqueue(change);
        }

        public override void GetSpriteLayerCount(Action<int> retrievalCallback)
        {
            SpriteGetLayerCount change = SpriteGetLayerCount.Get();
            change.RetrievalCallback = retrievalCallback;
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
            foreach (SpriteLayer layer in SpriteLayers)
            {
                foreach (ISprite s in layer.Sprites)
                    s.Render(renderer.SpriteBatcher);
            }
        }

        internal Matrix4F View = Matrix4F.Identity;

        internal Matrix4F Projection;

        internal Matrix4F ViewProjection;

        internal RenderSurfaceBase ChosenSurface;
    }
}
