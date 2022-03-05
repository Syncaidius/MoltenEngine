using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class ComputeTaskBinder : ContextSlotBinder<ComputeTask>
    {
        internal override void Bind(ContextSlot<ComputeTask> slot, ComputeTask value) { }

        internal override void Unbind(ContextSlot<ComputeTask> slot, ComputeTask value) { }
    }
}
