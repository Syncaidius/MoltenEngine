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
    internal unsafe class DeviceContextState : EngineObject
    {
        List<ContextSlot> _slots;

        internal DeviceContextState(DeviceContext context)
        {
            Context = context;
            _slots = new List<ContextSlot>();
            AllSlots = _slots.AsReadOnly();
        }

        internal void Clear()
        {
            Context.Native->ClearState();
        }

        internal ContextSlot<T> RegisterSlot<T, B>(PipeBindTypeFlags bindType, string namePrefix, uint slotIndex) 
            where T : PipeBindable
            where B : ContextBinder<T>, new()
        {
            B binder = new B();
            return RegisterSlot(bindType, namePrefix, slotIndex, binder);
        }

        internal ContextSlot<T> RegisterSlot<T>(PipeBindTypeFlags bindType, string namePrefix, uint slotIndex, ContextBinder<T> binder)
            where T : PipeBindable
        {
            ContextSlot<T> slot = new ContextSlot<T>(this, binder, bindType, namePrefix, slotIndex);
            _slots.Add(slot);
            return slot;
        }

        internal ContextSlotGroup<T> RegisterSlotGroup<T, B>(PipeBindTypeFlags bindType, string namePrefix, uint numSlots)
            where T : PipeBindable
            where B : ContextGroupBinder<T>, new()
        {
            B binder = new B();
            return RegisterSlotGroup(bindType, namePrefix, numSlots, binder);
        }

        internal ContextSlotGroup<T> RegisterSlotGroup<T>(PipeBindTypeFlags bindType, string namePrefix, uint numSlots, ContextGroupBinder<T> binder)
            where T : PipeBindable
        {
            ContextSlot<T>[] slots = new ContextSlot<T>[numSlots];
            ContextSlotGroup<T> grp = new ContextSlotGroup<T>(this, binder, slots, bindType, namePrefix);

            for (uint i = 0; i < numSlots; i++)
                slots[i] = new ContextSlot<T>(this, grp, bindType, namePrefix, i);

            _slots.AddRange(slots);

            return grp;
        }

        protected override void OnDispose() { }

        internal DeviceContext Context { get; }

        internal IReadOnlyList<ContextSlot> AllSlots { get; }
    }
}
