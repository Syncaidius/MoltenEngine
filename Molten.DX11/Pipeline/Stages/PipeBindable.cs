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
    internal abstract class PipeBindable : EngineObject
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
            if (!OnValidatebind(slot))
                return false;

            BoundTo.Add(slot);
            return true;
        }

        internal void UnbindFrom(PipeBindSlot slot)
        {
            BoundTo.Remove(slot);
        }

        protected abstract bool OnValidatebind(PipeBindSlot slot);

        internal protected uint Version { get; protected set; }

        internal PipeStageType CanBindTo { get; }

        internal PipeBindTypeFlags BindTypeFlags { get; }

        internal HashSet<PipeBindSlot> BoundTo { get; }
    }
}
