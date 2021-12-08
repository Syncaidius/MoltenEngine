using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents a DX11 bindable pipeline object.
    /// </summary>
    public abstract class PipeBindable : PipeObject
    {
        internal PipeBindable(DeviceDX11 device) : base(device)
        {
            BoundTo = new HashSet<PipeSlot>();
        }

        /// <summary>Invoked when the current <see cref="PipeBindable"/> is to be bound to a <see cref="PipeSlot"/>.</summary>
        /// <param name="slot">The <see cref="PipeSlot"/> to bind to.</param>
        /// <returns>True if the binding succeeded.</returns>
        internal bool BindTo(PipeSlot slot)
        {
            // TODO validate binding. Allow bindable to do it's own validation too.
            // If validation fails, return false here.
            // E.g. is the object bound to both input and outout in an invalid way?

            BoundTo.Add(slot);
            Refresh(slot, slot.Stage.Pipe);
            return true;
        }

        internal void UnbindFrom(PipeSlot slot)
        {
            BoundTo.Remove(slot);
        }

        /// <summary>
        /// Invoked right before the current <see cref="PipeBindable"/> is due to be bound to a <see cref="PipeDX11"/>.
        /// </summary>
        /// <param name="slot">The <see cref="PipeSlot"/> which contains the current <see cref="PipeBindable"/>.</param>
        /// <param name="pipe">The <see cref="PipeDX11"/> that the current <see cref="PipeBindable"/> is to be bound to.</param>
        protected internal abstract void Refresh(PipeSlot slot, PipeDX11 pipe);

        internal protected uint Version { get; protected set; }

        internal HashSet<PipeSlot> BoundTo { get; }
    }
}
