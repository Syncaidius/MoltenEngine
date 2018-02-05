using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="SceneObject"/> to the root of a scene.</summary>
    internal class EngineRemoveScene : EngineTask<EngineRemoveScene>
    {
        public Scene Scene;

        public override void Clear()
        {
            Scene = null;
        }

        public override void Process(Engine engine, Timing time)
        {
            engine.Scenes.Remove(Scene);
            Recycle(this);
        }
    }
}
