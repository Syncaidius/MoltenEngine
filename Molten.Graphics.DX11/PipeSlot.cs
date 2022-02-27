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

        internal abstract void Unbind();

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

        internal protected uint BindingID { get; protected set; }
    }

    internal sealed class PipeSlot<T> : PipeSlot
        where T : PipeBindable
    {
        uint _boundVersion;
        T _value;

        Action<PipeSlot<T>> _unbindCallback;

        internal PipeSlot(PipeStage stage, uint slotID, PipeBindTypeFlags slotType, string namePrefix, bool grpMember, Action<PipeSlot<T>> unbindCallback) : 
            base(stage, slotID, slotType, $"{namePrefix}_{typeof(T).Name}", grpMember)
        {

            _unbindCallback = unbindCallback;
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
                // Unbind old value
                if (BoundValue != null)
                {
                    // If the old value won't be replaced with a new one, unbind the value to empty the slot.
                    if (Value == null)
                        Unbind();

                    BoundValue?.UnbindFrom(this);
                }

                // Bind new value, if any.
                if (_value != null)
                {
                    // Unbind from slots that do not match the current slot's bind type
                    foreach (PipeSlot boundSlot in Value.BoundTo)
                    {
                        if (boundSlot.SlotType != SlotType)
                        {
                            // Only unbind if we have the newest binding.
                            if (boundSlot.BindingID < BindingID)
                            {
                                boundSlot.Unbind();
                                _value.UnbindFrom(boundSlot);
                            }
                            else
                            {
                                // Clear the slot, we don't have the latest binding.
                                if (BoundValue != null)
                                {
                                    Unbind();
                                    return false;
                                }
                            }
                        }
                    }

                    // Did the new bind fail?
                    if (!Value.BindTo(this))
                    {
                        BoundValue = null;
#if DEBUG
                        _value.Device.Log.WriteError($"Failed to bind {Value.Name} to {this.Name}");
#endif
                        return false;
                    }

                    _boundVersion = Value.Version;
                    BindingID = Value.BindingID;
                    BoundValue = Value;
                }

                Stage.Pipe.Profiler.Current.SlotBindings++;
                return true;
            }
            else if (Value != null)
            {
                if (BoundValue.BindTo(this))
                {
                    if (_boundVersion != Value.Version)
                    {
                        _boundVersion = Value.Version;
                        Stage.Pipe.Profiler.Current.GpuBindings++;
                        return true;
                    }
                }
            }

            return false;
        }

        internal override void Unbind()
        {
            _unbindCallback?.Invoke(this);
            if (Value == BoundValue)
                Value = null;

            BoundValue = null;
        }

        /// <summary>
        /// Gets or sets the value of the slot. This will be applied during the next pipe/context bind call.
        /// </summary>
        internal T Value
        {
            get => _value;
            set
            {
                _value = value;

                if (_value != null)
                    _value.BindingID++;
            }
        }

        /// <summary>
        /// Gets the value that was bound to the current <see cref="PipeSlot{T}"/> during the last <see cref="Bind"/> call.
        /// </summary>
        internal T BoundValue { get; set; }
    }
}
