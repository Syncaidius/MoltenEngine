using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal abstract class ContextSlot : EngineObject
    {
        internal ContextSlot(DeviceContextState parent, PipeBindTypeFlags bindType, string namePrefix, uint slotIndex)
        {
            ParentState = parent; 
            BindType = bindType;
            SlotIndex = slotIndex;
            Name = $"{namePrefix} slot {slotIndex}";
        }

        internal abstract bool Bind();

        internal abstract void Unbind();

        internal PipeBindTypeFlags BindType { get; }

        internal DeviceContextState ParentState { get; }

        internal uint SlotIndex { get; }

        internal DeviceContext Context => ParentState.Context;
    }

    internal class ContextSlot<T> : ContextSlot
        where T : ContextBindable
    {
        ContextBinder<T> _binder;
        ContextSlotGroup<T> _group;

        T _value;
        T _boundValue;

        uint _boundVersion;
        uint _pendingID;

        public ContextSlot(DeviceContextState parent, ContextBinder<T> binder, PipeBindTypeFlags bindType, string namePrefix, uint slotIndex) : 
            base(parent, bindType, $"{namePrefix}_{typeof(T).Name}", slotIndex)
        {
            IsGroupMember = false;
            _binder = binder;
        }

        public ContextSlot(DeviceContextState parent, ContextSlotGroup<T> grp, PipeBindTypeFlags bindType, string namePrefix, uint slotIndex) :
            base(parent, bindType, $"{namePrefix}_{typeof(T).Name}", slotIndex)
        {
            IsGroupMember = true;
            _group = grp;
            _binder = grp.Binder;
        }

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
                        if (slot._pendingID > _pendingID)
                        {
                            // Are the slot bind types different and does the object allow both simultaneously?
                            if (slot.BindType != BindType)
                            {
                                if ((_value.BindFlags & BindType) == BindType && (_value.BindFlags & slot.BindType) == slot.BindType)
                                    continue;
                            }

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


                    // If a value is bound in the current slot. Unbind it.
                    if (_boundValue != null && !canBind)
                    {
                        // TODO output _value.BoundTo list + details of current slot.
                        Unbind();
                        return false;
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

        protected override void OnDispose() { }

        internal T Value
        {
            get => _value;
            set
            {
                _value = value;
                _pendingID = _value.BindID++;
            }
        }

        internal T BoundValue => _boundValue;

        internal bool IsGroupMember { get; }
    }
}
