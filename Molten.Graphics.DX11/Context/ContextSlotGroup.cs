using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class ContextSlotGroup<T> : EngineObject
        where T : ContextBindable
    {
        ContextSlot<T>[] _slots;

        internal ContextSlotGroup(DeviceContextState parent, ContextGroupBinder<T> binder, ContextSlot<T>[] slots, PipeBindTypeFlags bindType, string namePrefix)
        {
            _slots = slots;
            Binder = binder;
            Name = $"{namePrefix} slot group";
            BindType = bindType;
            ParentState = parent;
        }

        protected override void OnDispose() { }

        internal ContextGroupBinder<T> Binder { get; }

        internal PipeBindTypeFlags BindType { get; }

        internal DeviceContextState ParentState { get; }

        internal DeviceContext Context => ParentState.Context;

        internal T this[uint slotIndex]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _slots[slotIndex].Value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _slots[slotIndex].Value = value;
        }
    }
}
