using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Represents a base for a critical GPU pipeline component.</summary>
    /// <seealso cref="Molten.EngineObject" />
    internal abstract class PipelineComponent : EngineObject
    {
        List<PipelineBindSlot> _slots;

        public PipelineComponent(GraphicsPipe pipe)
        {
            Pipe = pipe;
            Device = pipe.Device;
            _slots = new List<PipelineBindSlot>();
        }

        /// <summary>Adds a pipeline bind slot to the current <see cref="PipelineComponent"/>. 
        /// When the component is disposed, all <see cref="PipelineObject"/> bound to it will be automatically unbound.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="slotId">The slot identifier.</param>
        /// <returns></returns>
        internal PipelineBindSlot<T> AddSlot<T>(int slotId) where T : PipelineObject
        {
            PipelineBindSlot<T> slot = new PipelineBindSlot<T>(this, slotId);
            _slots.Add(slot);
            return slot;
        }

        protected override void OnDispose()
        {
            // Remove the slot bindings from their objects, if any.
            foreach (PipelineBindSlot slot in _slots)
                slot.Object?.Unbind(Pipe, slot);

            base.OnDispose();
        }

        protected void RefreshSlots()
        {
            for (int i = 0; i < _slots.Count; i++)
                _slots[i].Refresh(Pipe);
        }

        /// <summary>Gets the <see cref="GraphicsDeviceDX11"/> that the current <see cref="PipelineComponent"/> is bound to.</summary>
        public GraphicsDeviceDX11 Device { get; private set; }

        /// <summary>Gets the <see cref="GraphicsPipe"/> that the current <see cref="PipelineComponent"/> is bound to.</summary>
        internal GraphicsPipe Pipe { get; private set; }

        /// <summary>Gets whether or not the current <see cref="PipelineComponent"/> is in a valid state.</summary>
        internal virtual bool IsValid { get { return true; } }
    }
}
