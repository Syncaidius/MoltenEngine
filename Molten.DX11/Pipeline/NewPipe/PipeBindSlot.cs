using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class PipeBindSlot : EngineObject
    {
        internal PipeBindSlot(PipeStage parent, uint slotID, PipeBindTypeFlags slotType, string namePrefix)
        {
            Stage = parent;
            Index = slotID;
            SlotType = slotType;
            Name = $"{namePrefix} slot {Index}";
        }

        /// <summary>
        /// Invoked when the current <see cref="PipeBindSlot"/> should check/update value bindings.
        /// </summary>
        /// <returns></returns>
        internal abstract bool Bind();

        /// <summary>
        /// Gets the parent <see cref="PipeStage"/> that the current <see cref="PipeBindSlot"/> belongs to.
        /// </summary>
        internal PipeStage Stage { get; }

        /// <summary>
        /// Gets the slot index.
        /// </summary>
        internal uint Index { get; }

        /// <summary>
        /// Gets the slot type of the current <see cref="PipeBindSlot"/>.
        /// </summary>
        internal PipeBindTypeFlags SlotType { get; }
    }

    internal sealed class PipeBindSlot<T> : PipeBindSlot
        where T : PipeBindable
    {
        uint _boundVersion;

        internal PipeBindSlot(PipeStage stage, uint slotID, PipeBindTypeFlags slotType, string namePrefix) : 
            base(stage, slotID, slotType, $"{namePrefix}_{typeof(T).Name}")
        {

        }

        protected override void OnDispose()
        {
            BoundValue.UnbindFrom(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True, if changes were detected.</returns>
        internal override bool Bind()
        {
            if (BoundValue != Value)
            {
                BoundValue?.UnbindFrom(this);

                if (Value != null)
                {
                    // Did the new bind fail?
                    if (!Value.BindTo(this))
                    {
                        BoundValue = null;
#if DEBUG
                        Value.Device.Log.WriteError($"Failed to bind {Value.Name} to {this.Name}");
#endif
                        return false;
                    }

                    _boundVersion = Value.Version;
                    BoundValue = Value;
                }

                Stage.Pipe.Profiler.Current.Bindings++;
                return true;
            }
            else if (Value != null)
            {
                if (BoundValue.BindTo(this))
                {
                    if (_boundVersion != Value.Version)
                    {
                        _boundVersion = Value.Version;
                        Stage.Pipe.Profiler.Current.Bindings++;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets or sets the value of the slot. This will be applied during the next pipe/context bind call.
        /// </summary>
        internal T Value { get; set; }

        internal T BoundValue { get; set; }
    }
}
