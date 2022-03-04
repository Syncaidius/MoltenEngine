using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class MaterialBinder : ContextSlotBinder<Material>
    {
        internal override void Bind(ContextSlot<Material> slot, Material value) { }

        internal override void Unbind(ContextSlot<Material> slot, Material value) { }
    }
}
