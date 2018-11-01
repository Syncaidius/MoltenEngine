using System;
using System.Collections.Generic;
using System.Text;

namespace Molten.Graphics
{
    public interface IGraphicsDevice
    {
        void MarkForDisposal(PipelineDisposableObject obj);
    }
}
