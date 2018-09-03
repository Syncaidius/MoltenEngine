using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RendererReorderScene"/> for changing the draw order of a <see cref="SceneRenderDataDX11"/> instance.</summary>
    internal class RendererReorderScene : RendererTask<RendererReorderScene>
    {
        public SceneRenderData Data;
        public SceneReorderMode Mode;

        public override void Clear()
        {
            Data = null;
        }

        public override void Process(RendererDX11 renderer)
        {
            int indexOf = renderer.Scenes.IndexOf(Data);
            if (indexOf > -1)
            {
                renderer.Scenes.RemoveAt(indexOf);

                switch (Mode)
                {
                    case SceneReorderMode.PushBackward:
                            renderer.Scenes.Insert(Math.Max(0, indexOf - 1), Data);
                        break;

                    case SceneReorderMode.BringToFront:
                        renderer.Scenes.Add(Data);
                        break;

                    case SceneReorderMode.PushForward:
                        if (indexOf + 1 < renderer.Scenes.Count)
                            renderer.Scenes.Insert(indexOf + 1, Data);
                        else
                            renderer.Scenes.Add(Data);
                        break;

                    case SceneReorderMode.SendToBack:
                        renderer.Scenes.Insert(0, Data);
                        break;
                }
            }

            Recycle(this);
        }
    }

    internal enum SceneReorderMode
    {
        PushBackward,

        PushForward,

        SendToBack,

        BringToFront,
    }
}
