using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal delegate void PipelineBindSlotDelegate(PipelineBindSlot slot, PipelineObject obj);

    internal abstract class PipelineBindSlot
    {
        /// <summary>Invoked when the object bound to the slot is disposed.</summary>
        public event PipelineBindSlotDelegate OnBoundObjectDisposed;

        internal PipelineBindSlot(PipelineComponent parent, PipelineSlotType type, int slotID)
        {
            Type = type;
            SlotID = slotID;
            Parent = parent;
        }

        internal void BoundObjectDisposed(PipelineObject obj)
        {
            if (obj == Object)
            {
                OnBoundObjectDisposed?.Invoke(this, obj);
                UnbindDisposedObject(obj);
            }
        }

        /// <summary>Refreshes the object bound to the current slot, if any.</summary>
        /// <param name="pipe">The pipe.</param>
        internal void Refresh(GraphicsPipe pipe)
        {
            Object?.Refresh(pipe, this);
        }

        /// <summary>
        /// Occurs when the bound object is disposed.
        /// </summary>
        /// <param name="obj">The object which was just disposed</param>
        protected abstract void UnbindDisposedObject(PipelineObject obj);

        internal PipelineSlotType Type { get; private set; }

        internal int SlotID { get; private set; }

        internal PipelineComponent Parent { get; private set; }

        internal abstract PipelineObject Object { get; }
    }

    internal class PipelineBindSlot<T> : PipelineBindSlot where T : PipelineObject
    {
        internal PipelineBindSlot(PipelineComponent parent, PipelineSlotType type, int slotID) :
            base(parent, type, slotID)
        { }

        protected override void UnbindDisposedObject(PipelineObject obj)
        {
            BoundObject = null;
        }

        /// <summary>Binds a new object to the slot. If null, the existing object (if any) will be unbound.</summary>
        /// <param name="pipe">The <see cref="GraphicsPipe"/> to use to perform any binding operations..</param>
        /// <param name="obj">The <see cref="PipelineObject"/> to be bound to the object, or null to clear the existing one.</param>
        /// <returns></returns>
        internal bool Bind(GraphicsPipe pipe, T obj)
        {
            // Allow the new resource to refresh
            obj?.Refresh(pipe, this);

            // If the same resource is already bound, return false to signal no change.
            if (BoundObject == obj)
                return false;

            // If an object is already bound, unbind it, then bind the new one (if any).
            BoundObject?.Unbind(pipe, this);
            BoundObject = obj;
            obj?.Bind(pipe, this);

            // Return true to signal a difference between old and new object.
            pipe.Profiler.CurrentFrame.Bindings++;
            return true;
        }

        internal T BoundObject { get; private set; }

        internal override PipelineObject Object => BoundObject;
    }
}
