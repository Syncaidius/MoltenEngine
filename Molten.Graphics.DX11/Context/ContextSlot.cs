using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal abstract class ContextSlot : EngineObject
    {
        internal ContextSlot(DeviceContextState parent, ContextBindTypeFlags bindType, string namePrefix, uint slotIndex)
        {
            ParentState = parent; 
            BindType = bindType;
            SlotIndex = slotIndex;
            Name = $"{namePrefix} slot {slotIndex}";
        }

        internal abstract bool Bind();

        internal abstract void Unbind();

        internal ContextBindTypeFlags BindType { get; }

        internal DeviceContextState ParentState { get; }

        internal uint SlotIndex { get; }

        internal DeviceContext Context => ParentState.Context;
    }

    internal class ContextSlot<T> : ContextSlot
        where T : ContextBindable
    {
        ContextSlotBinder<T> _binder;
        ContextSlotGroup<T> _group;

        T _value;
        T _boundValue;

        uint _boundVersion;
        uint _pendingID;

        public ContextSlot(DeviceContextState parent, ContextSlotBinder<T> binder, ContextBindTypeFlags bindType, string namePrefix, uint slotIndex) : 
            base(parent, bindType, $"{namePrefix}_{typeof(T).Name}", slotIndex)
        {
            IsGroupMember = false;
            _group = null;
            _binder = binder;
        }

        public ContextSlot(DeviceContextState parent, ContextSlotGroup<T> grp, ContextBindTypeFlags bindType, string namePrefix, uint slotIndex) :
            base(parent, bindType, $"{namePrefix}_{typeof(T).Name}", slotIndex)
        {
            IsGroupMember = true;
            _group = grp;
            _binder = grp.Binder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if a binding changed occurred. False if no change has occurred.</returns>
        internal override bool Bind()
        {
            if (_value != _boundValue)
            {
                // If we're replacing a bound value with null, unbind it.
                if (_value == null)
                {
                    Unbind();
                }
                else
                {
                    // Check other bound slots that should be unbound.
                    bool canBind = true;
                    foreach (ContextSlot<T> slot in _value.BoundTo)
                    {
                        // Only check slots on the same context.
                        if (slot.Context != Context)
                            continue;

                        // Are the slot bind types different, or can the object to be simultaneously bound to input and output slots?
                        if (slot.BindType != BindType)
                        {
                            if ((_value.BindFlags & BindType) == BindType && (_value.BindFlags & slot.BindType) == slot.BindType)
                                continue;

                            if (slot._pendingID > _pendingID)
                            {
                                canBind = false;
                            }
                            else if (slot._pendingID < _pendingID)
                            {
                                slot.Unbind();
                            }
                            else if (slot._pendingID == _pendingID)
                            {
                                Context.Log.Error($"{_value.Name} is will be bound on '{slot.Name}' and '{Name}' with the same pending BindID ({_pendingID}). This is unexpected behaviour!");
                                canBind = false;
                            }
                        }
                    }

                    // If a value is bound in the current slot. Unbind it.
                    if (_boundValue != null && !canBind)
                    {
                        // TODO output _value.BoundTo list + details of current slot.
                        Unbind();
                        return true;
                    }

                    _value.Refresh(this, Context);
                    _boundValue = _value;
                    _boundVersion = _boundValue.Version;

                    if (!IsGroupMember)
                    {
                        _binder.Bind(this, _boundValue);
                        Context.Profiler.Current.GpuBindings++;
                    }

                    return true;
                }
            }
            else
            {
                // Refresh the existing value.
                if(_value != null)
                {
                    _value.Refresh(this, this.Context);
                    if (_boundVersion != Value.Version)
                    {
                        _boundVersion = Value.Version;
                        Context.Profiler.Current.GpuBindings++;
                        return true;
                    }
                }
            }

            return false;
        }

        internal override void Unbind()
        {
            _binder.Unbind(this, _boundValue);
            _boundValue = null;
        }

        public override string ToString()
        {
            return $"{Name} -- Value: {Value} -- Bound: {BoundValue}";
        }

        protected override void OnDispose() { }

        internal T Value
        {
            get => _value;
            set
            {
                _value = value;

                _pendingID = value != null ? _value.BindID++ : 0;
            }
        }

        internal T BoundValue => _boundValue;

        internal bool IsGroupMember { get; }
    }
}
