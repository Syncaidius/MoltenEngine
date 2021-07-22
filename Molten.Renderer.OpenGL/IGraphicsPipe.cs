using System;
using System.Collections.Generic;
using System.Text;

namespace Molten.Graphics
{
    public interface IGraphicsPipe<T> where T : IGraphicsDevice
    {
        T Device { get; }

        RenderProfiler Profiler { get; }
    }
}
