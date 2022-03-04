using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class ContextSlotBinder<T> 
        where T: PipeBindable
    {
        internal abstract void Bind(ContextSlot<T> slot, T value);

        internal abstract void Unbind(ContextSlot<T> slot, T value);
    }
}
