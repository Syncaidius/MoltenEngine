using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class SceneRenderData : EngineObject
    {
        public bool IsVisible = true;

        /// <summary>Gets or sets the camera that should be used as a view or eye when rendering a scene.</summary>
        public ICamera RenderCamera;

        public ICamera SpriteCamera;

        public Color BackgroundColor = new Color(20,20,20,255);

        public abstract void AddObject(IRenderable obj, ObjectRenderData renderData);

        public abstract void RemoveObject(IRenderable obj, ObjectRenderData renderData);

        public abstract void AddSprite(ISprite sprite, int layer = 0);

        public abstract void RemoveSprite(ISprite sprite, int layer = 0);

        public abstract void ChangeSpriteLayer(ISprite sprite, int oldLayer, int newLayer);

        /// <summary>Removes all <see cref="ISprite"/> instances from the specified layer.</summary>
        /// <param name="layer">The layer ID.</param>
        public abstract void ClearSpriteLayer(int layer);

        /// <summary>sets the visibility of a sprite layer. </summary>
        /// <param name="layer">The layer ID.</param>
        /// <param name="visible">If true, the layer will be visible. If false, it will not be visible.</param>
        public abstract void SetSpriteLayerVisibility(int layer, bool visible);

        /// <summary>Retrieves the IDs of visible sprite layers into the provided list.</summary>
        /// <param name="output"></param>
        public abstract void GetVisibleSpriteLayers(List<int> output, Action<List<int>> retrievalCallback);

        /// <summary>Sets the available number of layers. This can be used to both reduce or increase the maximum layer count.</summary>
        /// <param name="layerCount"></param>
        public abstract void SetSpriteLayerCount(int layerCount);

        /// <summary>Gets the last known number of sprite layers available.</summary>
        /// <returns></returns>
        public abstract void GetSpriteLayerCount(Action<int> retrievalCallback);
    }
}
