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

        internal protected abstract void Clear();

        internal ContextBindTypeFlags BindType { get; }

        internal DeviceContextState ParentState { get; }

        internal uint SlotIndex { get; }

        internal DeviceContext Context => ParentState.Context;

        internal protected uint PendingID { get; set; }

        internal protected abstract ContextBindable RawValue { get; }
    }

    internal class ContextSlot<T> : ContextSlot
        where T : ContextBindable
    {
        ContextSlotBinder<T> _binder;
        ContextSlotGroup<T> _group;

        T _value;
        T _boundValue;

        uint _boundVersion;

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
                    foreach (ContextSlot slot in _value.BoundTo)
                    {
                        // Only check slots on the same context.
                        if (slot.Context != Context)
                            continue;

                        // Are the slot bind types different, or can the object to be simultaneously bound to input and output slots?
                        if (slot.BindType != BindType)
                        {
                            if ((_value.BindFlags & BindType) == BindType && (_value.BindFlags & slot.BindType) == slot.BindType)
                                continue;

                            // If both slots will try to bind the same value, test which has higher priority...
                            if (slot.RawValue == _value)
                            {
                                if (slot.PendingID > PendingID)
                                {
                                    canBind = false;
                                }
                                else if (slot.PendingID < PendingID)
                                {
                                    slot.Clear();
                                }
                                else if (slot.PendingID == PendingID)
                                {
                                    Context.Log.Error($"{_value.Name} is will be bound on '{slot.Name}' and '{Name}' with the same pending BindID ({PendingID}). This is unexpected behaviour!");
                                    canBind = false;
                                }
                            }
                            else // ...Otherwise unbind the value from the other slot so this one can use it.
                            {
                                slot.Unbind();
                            }
                        }
                    }

                    if (!canBind)
                    {
                        if (_boundValue != null)
                        {
                            Unbind();
                            return true;
                        }

                        return false;
                    }
                    else
                    {
                        if (_boundValue != null)
                            _boundValue.BoundTo.Remove(this);
                    }

                    _value.Apply(Context);
                    _boundValue = _value;
                    _boundVersion = _boundValue.Version;
                    _value.BoundTo.Add(this);
                    Context.Log.Log($"Frame {Context.Device.Profiler.FrameCount} - Bound {_boundValue.Name} to {this.Name}");

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
                    _value.Apply(this.Context);
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
            _boundValue.BoundTo.Remove(this);
            Context.Log.Log($"Frame {Context.Device.Profiler.FrameCount} - Unbound {_boundValue.Name} from {Name}");
            _boundValue = null;
        }

        internal protected override void Clear()
        {
            if (_boundValue != null && _boundValue == _value)
                Unbind();

            _value = null;
            PendingID = 0;
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

                PendingID = value != null ? _value.BindID++ : 0;
            }
        }

        internal T BoundValue => _boundValue;

        internal protected override ContextBindable RawValue => _value;

        internal bool IsGroupMember { get; }
    }
}
