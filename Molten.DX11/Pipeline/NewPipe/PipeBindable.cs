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
        internal PipeBindable(PipeStageType canBindTo, PipeBindTypeFlags bindTypeFlags)
        {
            BindTypeFlags = bindTypeFlags;
            CanBindTo = canBindTo;
            BoundTo = new HashSet<PipeBindSlot>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot">The <see cref="PipeBindSlot"/> to bind to.</param>
        /// <returns>True if the binding succeeded.</returns>
        internal bool BindTo(PipeBindSlot slot)
        {
            BoundTo.Add(slot);
            OnBind(slot, slot.Stage.Pipe);
            return true;
        }

        internal void UnbindFrom(PipeBindSlot slot)
        {
            BoundTo.Remove(slot);
        }

        /// <summary>
        /// Invoked right before the current <see cref="PipeBindable"/> is due to be bound to a <see cref="PipeDX11"/>.
        /// </summary>
        /// <param name="slot">The <see cref="PipeBindSlot"/> which contains the current <see cref="PipeBindable"/>.</param>
        /// <param name="pipe">The <see cref="PipeDX11"/> that the current <see cref="PipeBindable"/> is to be bound to.</param>
        protected abstract void OnBind(PipeBindSlot slot, PipeDX11 pipe);

        internal protected uint Version { get; protected set; }

        internal PipeStageType CanBindTo { get; }

        internal PipeBindTypeFlags BindTypeFlags { get; }

        internal HashSet<PipeBindSlot> BoundTo { get; }
    }
}
