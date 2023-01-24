using System.Runtime.CompilerServices;

namespace Molten.Graphics
{
    internal class ContextSlotGroup<T> : EngineObject
        where T : ContextBindable
    {
        ContextSlot<T>[] _slots;

        internal ContextSlotGroup(CommandQueueDX11 queue, ContextGroupBinder<T> binder, ContextSlot<T>[] slots, GraphicsBindTypeFlags bindType, string namePrefix)
        {
            _slots = slots;
            Binder = binder;
            Name = $"{namePrefix} slot group";
            BindType = bindType;
            Cmd = queue;
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
                return true;
            }

            return false;
        }

        internal void Set(T[] values)
        {
            if (values.Length > _slots.Length)
                throw new Exception($"The provided array is larger than the group slot count");

            Set(values, 0, (uint)values.Length, 0);
        }

        internal void Set(T[] values, uint valueStartIndex, uint numValues, uint slotStartIndex)
        {
            uint valEndIndex = valueStartIndex + numValues;
            uint slotID = slotStartIndex;

            for (uint i = valueStartIndex; i < valEndIndex; i++)
            {
                if (values[i] != null)
                    throw new InvalidOperationException($"The provided buffer segment at index {i} is not part of a vertex buffer.");

                _slots[slotID++].Value = values[i];
            }
        }

        internal void Get(T[] destination)
        {
            // TODO rewrite group to store slot values in an array.
            // TODO add ContextGroupedSlot<T> : ContextSlot with group-specific implementation
            // TODO remove ContextSlot from groups
        }

        /// <summary>
        /// Unbinds all bound <see cref="ContextBindable"/> objects from the current <see cref="ContextSlotGroup{T}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnbindAll()
        {
            Binder.UnbindAll(this);
        }

        /// <summary>
        /// Gets the <see cref="ContextGroupBinder{T}"/> bound to the current <see cref="ContextSlotGroup{T}"/>.
        /// </summary>
        internal ContextGroupBinder<T> Binder { get; }

        /// <summary>
        /// Gets the bind type of the current <see cref="ContextSlotGroup{T}"/>.
        /// </summary>
        internal GraphicsBindTypeFlags BindType { get; }

        internal CommandQueueDX11 Cmd { get; }

        internal uint SlotCount { get; }

        internal ContextSlot<T> this[uint slotIndex]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _slots[slotIndex];
        }
    }
}
