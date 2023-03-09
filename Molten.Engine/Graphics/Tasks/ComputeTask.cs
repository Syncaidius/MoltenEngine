using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal class ComputeTask : RendererTask<ComputeTask>
    {
        internal HlslShader Shader;

        internal Vector3UI Groups;

        public override void ClearForPool()
        {
            Shader = null;
            Groups = Vector3UI.Zero;
        }

        public override void Process(RenderService renderer)
        {
            renderer.Device.Cmd.Dispatch(Shader, Groups);
            Recycle(this);
        }
    }
}
