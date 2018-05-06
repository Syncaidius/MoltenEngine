using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Represents a base for a critical GPU pipeline component.</summary>
    /// <seealso cref="Molten.EngineObject" />
    internal abstract class PipelineComponentGL : EngineObject
    {
        List<PipelineBindSlotGL> _slots;

        public PipelineComponentGL(GraphicsDeviceGL device)
        {
            Device = device;
            _slots = new List<PipelineBindSlotGL>();
        }

        /// <summary>Adds a pipeline bind slot to the current <see cref="PipelineComponent"/>. 
        /// When the component is disposed, all <see cref="PipelineObject"/> bound to it will be automatically unbound.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="slotId">The slot identifier.</param>
        /// <returns></returns>
        internal PipelineBindSlotGL<T> AddSlot<T>(int slotId) where T : PipelineObjectGL
        {
            PipelineBindSlotGL<T> slot = new PipelineBindSlotGL<T>(this, slotId);
            _slots.Add(slot);
            return slot;
        }

        protected override void OnDispose()
        {
            // Remove the slot bindings from their objects, if any.
            foreach (PipelineBindSlotGL slot in _slots)
                slot.Object?.Unbind(slot);

            base.OnDispose();
        }

        /// <summary>Gets the <see cref="GraphicsDeviceDX11"/> that the current <see cref="PipelineComponent"/> is bound to.</summary>
        public GraphicsDeviceGL Device { get; private set; }

        /// <summary>Gets whether or not the current <see cref="PipelineComponent"/> is in a valid state.</summary>
        internal virtual bool IsValid { get { return true; } }
    }
}
