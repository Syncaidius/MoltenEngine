﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class RunResourceTask : RenderTask<RunResourceTask>
    {
        internal IGraphicsResourceTask Task;

        internal GraphicsResource Resource;

        public override void ClearForPool()
        {
            Resource = null;
            Task = null;
        }

        public override void Process(RenderService renderer)
        {
            if (Task.Process(renderer.Device.Cmd, Resource))
                Resource.Version++;

            Recycle(this);
        }
    }
}