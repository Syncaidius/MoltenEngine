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
    public abstract class PipeBindable : EngineObject
    {
        internal PipeBindable(DeviceDX11 device)
        {
            Device = device;
            BoundTo = new HashSet<PipeBindSlot>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot">The <see cref="PipeBindSlot"/> to bind to.</param>
        /// <returns>True if the binding succeeded.</returns>
        internal bool BindTo(PipeBindSlot slot)
        {
            // TODO validate binding. Allow bindable to do it's own validation too.
            // If validation fails, return false here.
            // E.g. is the object bound to both input and outout in an invalid way?

            BoundTo.Add(slot);
            Refresh(slot, slot.Stage.Pipe);
            return true;
        }

        internal void UnbindFrom(PipeBindSlot slot)
        {
            BoundTo.Remove(slot);
        }

        protected override sealed void OnDispose()
        {
            Device.MarkForDisposal(this);
        }

        internal abstract void PipelineDispose();

        /// <summary>
        /// Invoked right before the current <see cref="PipeBindable"/> is due to be bound to a <see cref="PipeDX11"/>.
        /// </summary>
        /// <param name="slot">The <see cref="PipeBindSlot"/> which contains the current <see cref="PipeBindable"/>.</param>
        /// <param name="pipe">The <see cref="PipeDX11"/> that the current <see cref="PipeBindable"/> is to be bound to.</param>
        protected internal abstract void Refresh(PipeBindSlot slot, PipeDX11 pipe);

        internal protected uint Version { get; protected set; }

        internal HashSet<PipeBindSlot> BoundTo { get; }

        public DeviceDX11 Device { get; }
    }
}
