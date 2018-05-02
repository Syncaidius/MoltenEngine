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
        public event PipelineBindSlotDelegate OnObjectForcedUnbind;

        internal PipelineBindSlot(PipelineComponent parent, int slotID)
        {
            SlotID = slotID;
            Parent = parent;
        }

        internal void BoundObjectDisposed(PipelineObject obj)
        {
            if (obj == Object)
            {
                OnObjectForcedUnbind?.Invoke(this, obj);
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
        /// Triggers a mandatory unbinding of any object currently bound to the slot.
        /// </summary>
        /// <param name="pipe"></param>
        internal void ForceUnbind(GraphicsPipe pipe)
        {
            OnObjectForcedUnbind?.Invoke(this, Object);
            OnForceUnbind(pipe);
        }

        /// <summary>
        /// Occurs when the bound object is disposed.
        /// </summary>
        /// <param name="obj">The object which was just disposed</param>
        protected abstract void UnbindDisposedObject(PipelineObject obj);

        protected abstract void OnForceUnbind(GraphicsPipe pipe);

        internal int SlotID { get; private set; }

        internal PipelineComponent Parent { get; private set; }

        internal abstract PipelineObject Object { get; }

        internal PipelineBindType BindType { get; private protected set; }
    }

    internal class PipelineBindSlot<T> : PipelineBindSlot where T : PipelineObject
    {
        internal PipelineBindSlot(PipelineComponent parent, int slotID) :
            base(parent, slotID)
        { }

        protected override void UnbindDisposedObject(PipelineObject obj)
        {
            BoundObject = null;
        }

        protected override void OnForceUnbind(GraphicsPipe pipe)
        {
            BoundObject?.Unbind(pipe, this);
            BoundObject = null;
        }

        /// <summary>Binds a new object to the slot. If null, the existing object (if any) will be unbound.</summary>
        /// <param name="pipe">The <see cref="GraphicsPipe"/> to use to perform any binding operations..</param>
        /// <param name="obj">The <see cref="PipelineObject"/> to be bound to the object, or null to clear the existing one.</param>
        /// <returns></returns>
        internal bool Bind(GraphicsPipe pipe, T obj, PipelineBindType bindType)
        {
            if(obj != null)
            {
                obj.Refresh(pipe, this);
                if (BoundObject == obj && BindType == bindType)
                    return false;

                BoundObject?.Unbind(pipe, this);
                BoundObject = obj;
                BindType = bindType;
                obj.Bind(pipe, this);
            }
            else
            {
                if (BoundObject == null && BindType == bindType)
                    return false;

                BoundObject?.Unbind(pipe, this);
                BoundObject = obj;
                BindType = bindType;
            }            

            // Return true to signal a difference between old and new object.
            pipe.Profiler.CurrentFrame.Bindings++;
            return true;
        }

        internal T BoundObject { get; private set; }

        internal override PipelineObject Object => BoundObject;
    }
}
