using System.Runtime.CompilerServices;

namespace Molten.Graphics
{
    public class GraphicsSlotGroup<T> : EngineObject
        where T : class, IGraphicsObject
    {
        GraphicsSlot<T>[] _slots;

        public GraphicsSlotGroup(GraphicsCommandQueue queue, GraphicsGroupBinder<T> binder, GraphicsSlot<T>[] slots, GraphicsBindTypeFlags bindType, string namePrefix)
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
        /// Binds all pending <see cref="GraphicsObject"/> objects on to the current <see cref="GraphicsSlotGroup{T}"/>
        /// </summary>
        public bool BindAll()
        {
            uint firstChanged = uint.MaxValue;
            uint lastChanged = uint.MinValue;

            foreach (GraphicsSlot<T> slot in _slots)
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

        public void Set(T[] values)
        {
            if (values.Length > _slots.Length)
                throw new Exception($"The provided array is larger than the group slot count");

            Set(values, 0, (uint)values.Length, 0);
        }

        public void Set(T[] values, uint valueStartIndex, uint numValues, uint slotStartIndex)
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

        public void Get(T[] destination)
        {
            // TODO rewrite group to store slot values in an array.
            // TODO add ContextGroupedSlot<T> : ContextSlot with group-specific implementation
            // TODO remove ContextSlot from groups
        }

        /// <summary>
        /// Unbinds all bound <see cref="GraphicsObject"/> objects from the current <see cref="GraphicsSlotGroup{T}"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void UnbindAll()
        {
            Binder.UnbindAll(this);
        }

        /// <summary>
        /// Gets the <see cref="GraphicsGroupBinder{T}"/> bound to the current <see cref="GraphicsSlotGroup{T}"/>.
        /// </summary>
        internal GraphicsGroupBinder<T> Binder { get; }

        /// <summary>
        /// Gets the bind type of the current <see cref="GraphicsSlotGroup{T}"/>.
        /// </summary>
        public GraphicsBindTypeFlags BindType { get; }

        /// <summary>
        /// Gets the <see cref="GraphicsCommandQueue"/> that the current <see cref="GraphicsSlotGroup{T}"/> is bound to.
        /// </summary>
        public GraphicsCommandQueue Cmd { get; }

        /// <summary>
        /// Gets the number of <see cref="GraphicsSlot{T}"/> in the current <see cref="GraphicsSlotGroup{T}"/>.
        /// </summary>
        public uint SlotCount { get; }

        /// <summary>
        /// Gets the <see cref="GraphicsSlot{T}"/> at a given index.
        /// </summary>
        /// <param name="slotIndex">The slot index.</param>
        /// <returns></returns>
        public GraphicsSlot<T> this[uint slotIndex]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _slots[slotIndex];
        }
    }
}
