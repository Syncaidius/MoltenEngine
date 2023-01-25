namespace Molten.Graphics
{
    public abstract class GraphicsSlot : EngineObject
    {
        internal GraphicsSlot(GraphicsCommandQueue cmd, GraphicsBindTypeFlags bindType, string namePrefix, uint slotIndex)
        {
            Cmd = cmd; 
            BindType = bindType;
            SlotIndex = slotIndex;
            Name = $"{namePrefix}_slot_{slotIndex}";
        }

        public abstract bool Bind();

        public abstract void Unbind();

        public abstract void Clear();

        public GraphicsBindTypeFlags BindType { get; }

        public uint SlotIndex { get; }

        public GraphicsCommandQueue Cmd { get; }

        public uint PendingID { get; internal set; }

        public abstract GraphicsObject RawValue { get; }
    }

    public class GraphicsSlot<T> : GraphicsSlot
        where T : GraphicsObject
    {
        GraphicsSlotBinder<T> _binder;
        GraphicsSlotGroup<T> _group;

        T _value;
        T _boundValue;

        uint _boundVersion;

        public GraphicsSlot(GraphicsCommandQueue queue, GraphicsSlotBinder<T> binder, GraphicsBindTypeFlags bindType, string namePrefix, uint slotIndex) : 
            base(queue, bindType, namePrefix, slotIndex)
        {
            IsGroupMember = false;
            _group = null;
            _binder = binder;
        }

        public GraphicsSlot(GraphicsCommandQueue queue, GraphicsSlotGroup<T> grp, GraphicsBindTypeFlags bindType, string namePrefix, uint slotIndex) :
            base(queue, bindType, namePrefix, slotIndex)
        {
            IsGroupMember = true;
            _group = grp;
            _binder = grp.Binder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if a binding changed occurred. False if no change has occurred.</returns>
        public override bool Bind()
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
                    GraphicsSlot slot = null;

                    for (int i = _value.BoundTo.Count - 1; i >= 0; i--)
                    {
                        slot = _value.BoundTo[i];

                        // Only check slots on the same context.
                        if (slot.Cmd != Cmd)
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
                                    Cmd.Device.Log.Error($"{_value.Name} is will be bound on '{slot.Name}' and '{Name}' with the same pending BindID ({PendingID}). This is unexpected behaviour!");
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

                    _value.Apply(Cmd);
                    _boundValue = _value;
                    _boundVersion = _boundValue.Version;
                    _value.BoundTo.Add(this);

                    if (!IsGroupMember)
                    {
                        _binder.Bind(this, _boundValue);
                        Cmd.Profiler.Current.GpuBindings++;
                    }
                }

                return true;
            }
            else
            {
                // Refresh the existing value.
                if(_value != null)
                {
                    _value.Apply(this.Cmd);
                    if (_boundVersion != Value.Version)
                    {
                        _boundVersion = Value.Version;
                        Cmd.Profiler.Current.GpuBindings++;
                        return true;
                    }
                }
            }

            return false;
        }

        public override void Unbind()
        {
            _binder.Unbind(this, _boundValue);
            _boundValue.BoundTo.Remove(this);
            _boundValue = null;
        }

        public override void Clear()
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

        public T Value
        {
            get => _value;
            set
            {
                _value = value;

                PendingID = value != null ? _value.BindID++ : 0;
            }
        }

        public T BoundValue => _boundValue;

        public override GraphicsObject RawValue => _value;

        internal bool IsGroupMember { get; }
    }
}
