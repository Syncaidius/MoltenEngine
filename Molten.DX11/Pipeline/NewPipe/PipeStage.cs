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
        internal PipeStage(PipeDX11 pipe, PipeStageType stageType)
        {
            Pipe = pipe;
            StageType = stageType;
            AllSlots = new List<PipeBindSlot>();
        }

        protected PipeBindSlotGroup<T> DefineSlotGroup<T>(uint slotCount, PipeBindTypeFlags slotType, string namePrefix)
            where T : PipeBindable
        {
            return new PipeBindSlotGroup<T>(this, slotCount, slotType, namePrefix);
        }

        protected PipeBindSlot<T> DefineSlot<T>(uint slotID, PipeBindTypeFlags slotType, string namePrefix)
            where T : PipeBindable
        {
            PipeBindSlot<T> slot = new PipeBindSlot<T>(this, slotID, slotType, namePrefix);
            AllSlots.Add(slot);
            return slot;
        }

        protected override void OnDispose()
        {
            foreach (PipeBindSlot slot in AllSlots)
                slot.Dispose();
        }

        /// <summary>
        /// Bind the stage to it's <see cref="ID3D11DeviceContext1"/> context.
        /// </summary>
        internal abstract void Bind();

        internal PipeDX11 Pipe { get; }

        internal DeviceDX11 Device => Pipe.Device;

        /// <summary>
        /// Gets the type of the current <see cref="PipeStage"/>.
        /// </summary>
        internal PipeStageType StageType { get; }

        internal List<PipeBindSlot> AllSlots { get; }
    }
}
