using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class SpriteSetLayerVisibility : RenderSceneChange<SpriteSetLayerVisibility> 
    {
        public int LayerID;

        public bool Visibility;

        public override void Clear() { }

        public override void Process(SceneRenderDataDX11 scene)
        {
            if (LayerID >= scene.SpriteLayers.Length)
                return;

            scene.SpriteLayers[LayerID].Visible = Visibility;

            Recycle(this);
        }
    }
}
