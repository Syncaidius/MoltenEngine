using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class PipeSlotGroup<T>
        where T : PipeBindable
    {
        public delegate void PipeBindSlotGroupCallback(PipeSlotGroup<T> grp, uint numChanged);

        PipeSlot<T>[] _slots;

        internal PipeSlotGroup(PipeStage stage, uint slotCount, PipeBindTypeFlags slotType, string namePrefix)
        {
            _slots = new PipeSlot<T>[slotCount];

            for (uint i = 0; i < slotCount; i++)
                _slots[i] = new PipeSlot<T>(stage, i, slotType, namePrefix, true);

            stage.AllSlots.AddRange(_slots);
        }

        public bool BindAll()
        {
            // Reset trackers
            FirstChanged = uint.MaxValue;
            LastChanged = 0;
            NumSlotsChanged = 0;

            foreach(PipeSlot<T> slot in _slots)
            {
                if (slot.Bind())
                {
                    if (slot.Index < FirstChanged)
                        FirstChanged = slot.Index;

                    if (slot.Index > LastChanged)
                        LastChanged = slot.Index;
                }
            }

            // If first slot is less than last slot, changes occurred.
            if(FirstChanged < LastChanged)
            {
                NumSlotsChanged = LastChanged - FirstChanged;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the <see cref="PipeSlot{T}"/> at the specified slot ID/Index, 
        /// within the current <see cref="PipeSlotGroup{T}"/>.
        /// </summary>
        /// <param name="index">The slot ID/index.</param>
        /// <returns></returns>
        public PipeSlot<T> this[uint index] => _slots[index];

        /// <summary>
        /// Gets the first <see cref="PipeSlot{T}"/> in the current 
        /// <see cref="PipeSlotGroup{T}"/> that was changed, during the previous <see cref="BindAll"/> call.
        /// </summary>
        public uint FirstChanged { get; private set; }

        /// <summary>
        /// Gets the last <see cref="PipeSlot{T}"/> in the current 
        /// <see cref="PipeSlotGroup{T}"/> that was changed, during the previous <see cref="BindAll"/> call.
        /// </summary>
        public uint LastChanged { get; private set; }

        /// <summary>
        /// Gets the number of slots that were changed during the last <see cref="BindAll"/> call.
        /// </summary>
        public uint NumSlotsChanged { get; private set; }

        /// <summary>
        /// Gets the <see cref="PipeStage"/> that the current <see cref="PipeSlotGroup{T}"/> is bound to.
        /// </summary>
        public PipeStage Stage { get; }

        /// <summary>
        /// Gets the number of slots in the current <see cref="PipeSlotGroup{T}"/>.
        /// </summary>
        public int SlotCount => _slots.Length;
    }
}
