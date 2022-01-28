using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// Represents a managed device context pipeline stage.
    /// </summary>
    internal unsafe abstract class PipeStage : EngineObject
    {
        internal PipeStage(DeviceContext pipe)
        {
            Pipe = pipe;
            AllSlots = new List<PipeSlot>();
        }

        protected PipeSlotGroup<T> DefineSlotGroup<T>(uint slotCount, PipeBindTypeFlags slotType, string namePrefix)
            where T : PipeBindable
        {
            return new PipeSlotGroup<T>(this, slotCount, slotType, namePrefix);
        }

        protected PipeSlot<T> DefineSlot<T>(uint slotID, PipeBindTypeFlags slotType, string namePrefix)
            where T : PipeBindable
        {
            PipeSlot<T> slot = new PipeSlot<T>(this, slotID, slotType, namePrefix, false);
            AllSlots.Add(slot);
            return slot;
        }

        protected override void OnDispose()
        {
            foreach (PipeSlot slot in AllSlots)
                slot.Dispose();
        }

        internal DeviceContext Pipe { get; }

        internal Device Device => Pipe.Device;

        internal List<PipeSlot> AllSlots { get; }
    }
}
