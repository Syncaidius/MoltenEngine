using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class PipeBindSlotGroup<T>
        where T : PipeBindable
    {
        public delegate void PipeBindSlotGroupCallback(PipeBindSlot<T>[] slots, uint startSlot, uint endSlot, uint numChanged);

        PipeBindSlot<T>[] _slots;

        internal PipeBindSlotGroup(PipeStage stage, uint slotCount, PipeBindTypeFlags slotType, string namePrefix)
        {
            _slots = new PipeBindSlot<T>[slotCount];

            for (uint i = 0; i < slotCount; i++)
                _slots[i] = new PipeBindSlot<T>(stage, i, slotType, namePrefix);

            stage.AllSlots.AddRange(_slots);
        }

        public bool BindAll(PipeBindSlotGroupCallback bindCallback)
        {
            // Reset trackers
            FirstChanged = uint.MaxValue;
            LastChanged = uint.MinValue;

            foreach(PipeBindSlot<T> slot in _slots)
            {
                if (slot.Bind())
                {
                    FirstChanged = slot.SlotID < FirstChanged ? slot.SlotID : FirstChanged;
                    LastChanged = slot.SlotID > LastChanged ? slot.SlotID : LastChanged;
                }
            }

            // If first slot is less than last slot, changes occurred.
            if(FirstChanged < LastChanged)
            {
                uint numChanged = LastChanged - FirstChanged;
                bindCallback?.Invoke(_slots, FirstChanged, LastChanged, numChanged);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the first <see cref="PipeBindSlot{T}"/> in the current 
        /// <see cref="PipeBindSlotGroup{T}"/> that was changed, during the previous <see cref="BindAll"/> call.
        /// </summary>
        public uint FirstChanged { get; private set; }

        /// <summary>
        /// Gets the last <see cref="PipeBindSlot{T}"/> in the current 
        /// <see cref="PipeBindSlotGroup{T}"/> that was changed, during the previous <see cref="BindAll"/> call.
        /// </summary>
        public uint LastChanged { get; private set; }
    }
}
