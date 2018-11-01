using System;
using System.Collections.Generic;
using System.Text;

namespace Molten.Graphics
{
    public abstract class PipelineDisposableObject : EngineObject
    {
        internal abstract void PipelineDispose();
    }
}
