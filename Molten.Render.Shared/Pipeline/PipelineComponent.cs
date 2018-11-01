using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Represents a base for a critical GPU pipeline component.</summary>
    /// <seealso cref="Molten.EngineObject" />
    internal abstract class PipelineComponent<D, P> : EngineObject
        where D: IGraphicsDevice
        where P : IGraphicsPipe<D>
    {
        List<PipelineBindSlot<D,P>> _slots;

        public PipelineComponent(P pipe)
        {
            Pipe = pipe;
            Device = pipe.Device;
            _slots = new List<PipelineBindSlot<D, P>>();
        }

        /// <summary>Adds a pipeline bind slot to the current <see cref="PipelineComponent"/>. 
        /// When the component is disposed, all <see cref="PipelineDisposableObject"/> bound to it will be automatically unbound.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <param name="slotId">The slot identifier.</param>
        /// <returns></returns>
        internal PipelineBindSlot<T, D,P> AddSlot<T>(int slotId)  where T : PipelineObject<D,P>
        {
            PipelineBindSlot<T, D, P> slot = new PipelineBindSlot<T, D, P>(this, slotId);
            _slots.Add(slot);
            return slot;
        }

        protected override void OnDispose()
        {
            // Remove the slot bindings from their objects, if any.
            foreach (PipelineBindSlot<D, P> slot in _slots)
                slot.Object?.Unbind(slot);

            base.OnDispose();
        }

        /// <summary>Gets the <see cref="DeviceDX11"/> that the current <see cref="PipelineComponent"/> is bound to.</summary>
        public D Device { get; private set; }

        /// <summary>Gets the <see cref="PipeDX11"/> that the current <see cref="PipelineComponent"/> is bound to.</summary>
        internal P Pipe { get; private set; }

        /// <summary>Gets whether or not the current <see cref="PipelineComponent"/> is in a valid state.</summary>
        internal virtual bool IsValid { get { return true; } }
    }
}
