using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents the complete state of a <see cref="DeviceContext"/>.
    /// </summary>
    internal class DeviceContextState : EngineObject
    {
        List<ContextSlot> _slots;
        Dictionary<Type, object> _binders;

        internal DeviceContextState(DeviceContext context)
        {
            Context = context;
            _slots = new List<ContextSlot>();
            _binders = new Dictionary<Type, object>();
            AllSlots = _slots.AsReadOnly();
        }

        internal ContextSlot<T> RegisterSlot<T, B>(PipeBindTypeFlags bindType, string namePrefix, uint slotIndex) 
            where T : ContextBindable
            where B : ContextBinder<T>, new()
        {
            Type binderType = typeof(B);

            if (!_binders.TryGetValue(binderType, out object binder)) {
                binder = new B();
                _binders.Add(binderType, binder);
            }

            ContextSlot<T> slot = new ContextSlot<T>(this, binder as B, bindType, namePrefix, slotIndex);
            _slots.Add(slot);
            return slot;
        }

        internal ContextSlotGroup<T> RegisterSlotGroup<T, B>(PipeBindTypeFlags bindType, string namePrefix, uint numSlots)
            where T : ContextBindable
            where B : ContextGroupBinder<T>, new()
        {
            Type binderType = typeof(B);

            if (!_binders.TryGetValue(binderType, out object binder))
            {
                binder = new B();
                _binders.Add(binderType, binder);
            }

            ContextSlot<T>[] slots = new ContextSlot<T>[numSlots];
            ContextSlotGroup<T> grp = new ContextSlotGroup<T>(this, binder as B, slots, bindType, namePrefix);

            for(uint i = 0; i < numSlots; i++)
                slots[i] = new ContextSlot<T>(this, grp, bindType, namePrefix, i);

            _slots.AddRange(slots);

            return grp;
        }

        protected override void OnDispose() { }

        internal DeviceContext Context { get; }

        internal IReadOnlyList<ContextSlot> AllSlots { get; }
    }
}
