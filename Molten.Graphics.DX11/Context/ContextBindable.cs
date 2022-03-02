using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class ContextBindable : ContextStateObject
    {
        internal HashSet<ContextSlot> BoundTo { get; }

        internal ContextBindable(Device device, PipeBindTypeFlags bindFlags) :
            base(device)
        {
            BoundTo = new HashSet<ContextSlot>();
        }

        internal abstract void Refresh(ContextSlot slot, DeviceContext context); 

        internal uint BindID { get; set; }
    }
}
