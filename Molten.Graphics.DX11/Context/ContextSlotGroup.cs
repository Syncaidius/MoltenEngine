using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class ContextSlotGroup<T> : EngineObject
        where T : PipeBindable
    {
        ContextSlot<T>[] _slots;

        internal ContextSlotGroup(DeviceContextState parent, ContextGroupBinder<T> binder, ContextSlot<T>[] slots, PipeBindTypeFlags bindType, string namePrefix)
        {
            _slots = slots;
            Binder = binder;
            Name = $"{namePrefix} slot group";
            BindType = bindType;
            ParentState = parent;
            SlotCount = (uint)slots.Length;
        }

        protected override void OnDispose() { }

        /// <summary>
        /// Binds all pending <see cref="ContextBindable"/> objects on to the current <see cref="ContextSlotGroup{T}"/>
        /// </summary>
        internal bool BindAll()
        {
            uint firstChanged = uint.MaxValue;
            uint lastChanged = uint.MinValue;

            foreach (ContextSlot<T> slot in _slots)
            {
                if (slot.Bind())
                {
                    if (slot.SlotIndex < firstChanged)
                        firstChanged = slot.SlotIndex;

                    if (slot.SlotIndex > lastChanged)
                        lastChanged = slot.SlotIndex;
                }
            }

            // If first slot is less than or equal last slot, changes occurred.
            if (firstChanged <= lastChanged)
            {
                uint numChanged = (1 + lastChanged) - firstChanged;
                Binder.Bind(this, firstChanged, lastChanged, numChanged);
            }

            return false;
        }

        /// <summary>
        /// Unbinds all bound <see cref="ContextBindable"/> objects from the current <see cref="ContextSlotGroup{T}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnbindAll()
        {
            Binder.UnbindAll(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T GetBoundValue(uint slotIndex)
        {
            return _slots[slotIndex].BoundValue;
        }

        /// <summary>
        /// Gets the <see cref="ContextGroupBinder{T}"/> bound to the current <see cref="ContextSlotGroup{T}"/>.
        /// </summary>
        internal ContextGroupBinder<T> Binder { get; }

        /// <summary>
        /// Gets the bind type of the current <see cref="ContextSlotGroup{T}"/>.
        /// </summary>
        internal PipeBindTypeFlags BindType { get; }

        /// <summary>
        /// Gets the parent <see cref="DeviceContextState"/> of the current <see cref="ContextSlotGroup{T}"/>.
        /// </summary>
        internal DeviceContextState ParentState { get; }

        internal DeviceContext Context => ParentState.Context;

        internal uint SlotCount { get; }


        internal T this[uint slotIndex]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _slots[slotIndex].Value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _slots[slotIndex].Value = value;
        }
    }
}
