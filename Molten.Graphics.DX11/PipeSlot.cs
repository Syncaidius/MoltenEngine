using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class PipeSlot : EngineObject
    {
        internal PipeSlot(PipeStage parent, uint slotID, PipeBindTypeFlags slotType, string namePrefix, bool grpMember)
        {
            IsGroupMember = grpMember;
            Stage = parent;
            Index = slotID;
            SlotType = slotType;
            Name = $"{namePrefix} slot {Index}";
        }

        /// <summary>
        /// Invoked when the current <see cref="PipeSlot"/> should check/update value bindings.
        /// </summary>
        /// <returns></returns>
        internal abstract bool Bind();

        /// <summary>
        /// Gets the parent <see cref="PipeStage"/> that the current <see cref="PipeSlot"/> belongs to.
        /// </summary>
        internal PipeStage Stage { get; }

        /// <summary>
        /// Gets the slot index.
        /// </summary>
        internal uint Index { get; }

        /// <summary>
        /// Gets the slot type of the current <see cref="PipeSlot"/>.
        /// </summary>
        internal PipeBindTypeFlags SlotType { get; }

        /// <summary>
        /// Gets whether or not the slot is part of a pipe slot group.
        /// </summary>
        public bool IsGroupMember { get; }
    }

    internal sealed class PipeSlot<T> : PipeSlot
        where T : PipeBindable
    {
        uint _boundVersion;
        uint _bindIncrement;

        internal PipeSlot(PipeStage stage, uint slotID, PipeBindTypeFlags slotType, string namePrefix, bool grpMember) : 
            base(stage, slotID, slotType, $"{namePrefix}_{typeof(T).Name}", grpMember)
        {
            _bindIncrement = grpMember ? 0 : 1U;
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

                Stage.Pipe.Profiler.Current.Bindings += _bindIncrement;
                return true;
            }
            else if (Value != null)
            {
                if (BoundValue.BindTo(this))
                {
                    if (_boundVersion != Value.Version)
                    {
                        _boundVersion = Value.Version;
                        Stage.Pipe.Profiler.Current.Bindings += _bindIncrement;
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

        /// <summary>
        /// Gets the value that was bound to the current <see cref="PipeSlot{T}"/> during the last <see cref="Bind"/> call.
        /// </summary>
        internal T BoundValue { get; set; }
    }
}
