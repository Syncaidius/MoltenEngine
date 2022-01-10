using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal enum DeviceCreationFlags
    {
        SingleThreaded,
        Debug,
        SwitchToRef,
        PreventInternalThreadingOptimizations,
        BgraSupport,
        Debuggable,
        PreventAlteringLayerSettingsFromRegistery,
        DisableGpuTimeout,
        VideoSupport,
    };
}
