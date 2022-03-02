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
        uint _boundID;

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
                    foreach(ContextSlot<T> slot in _value.BoundTo)
                    {
                        if(slot._boundID > _boundID)
                        {
                            // If a value is bound in the current slot. Unbind it.
                            if (_boundValue != null)
                            {
                                Context.Log.Debug($"Attempt to bind '{_value.Name}' to '{Name}' failed: Already bound to '{slot.Name}'");
                                Unbind();
                            }

                            return false;
                        } 
                    }

                    _value.Refresh(this, Context);
                    _boundValue = _value;
                    _boundID = _boundValue.BindID;
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
                _value.BindID++;
            }
        }

        internal T BoundValue => _boundValue;

        internal bool IsGroupMember { get; }
    }
}
