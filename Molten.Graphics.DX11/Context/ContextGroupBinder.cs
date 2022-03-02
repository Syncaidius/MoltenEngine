using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class ContextGroupBinder<T> : ContextBinder<T>
        where T : ContextBindable
    {
        internal abstract bool Bind(ContextSlotGroup<T> grp, uint startIndex, uint endIndex, uint numChanged);

        internal abstract void Unbind(ContextSlotGroup<T> grp, uint startIndex, uint endIndex, uint numChanged);
    }
}
