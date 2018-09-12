using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>A <see cref="RendererReorderScene"/> for changing the draw order of a <see cref="SceneRenderDataDX11"/> instance.</summary>
    internal class SceneLayerReorder : SceneChange<SceneLayerReorder>
    {
        public SceneLayer Layer;
        public SceneReorderMode Mode;

        public override void Clear()
        {
            Layer = null;
        }

        internal override void Process(Scene scene)
        {
            int indexOf = scene.Layers.IndexOf(Layer);
            if (indexOf > -1)
            {
                scene.Layers.RemoveAt(indexOf);

                switch (Mode)
                {
                    case SceneReorderMode.PushBackward:
                        scene.Layers.Insert(Math.Max(0, indexOf - 1), Layer);
                        break;

                    case SceneReorderMode.BringToFront:
                        scene.Layers.Add(Layer);
                        break;

                    case SceneReorderMode.PushForward:
                        if (indexOf + 1 < scene.Layers.Count)
                            scene.Layers.Insert(indexOf + 1, Layer);
                        else
                            scene.Layers.Add(Layer);
                        break;

                    case SceneReorderMode.SendToBack:
                        scene.Layers.Insert(0, Layer);
                        break;
                }
            }

            Recycle(this);
        }
    }
}
