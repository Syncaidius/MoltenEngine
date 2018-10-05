using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A <see cref="RenderSceneChange"/> for adding a <see cref="ICamera"/> to a scene.</summary>
    internal class AddCamera : RenderSceneChange<AddCamera> 
    {
        public RenderCamera Camera;
        public SceneRenderData Data;

        public override void Clear()
        {
            Camera = null;
            Data = null;
        }

        public override void Process()
        {
            Data.Cameras.Add(Camera);
            Recycle(this);
        }
    }
}
