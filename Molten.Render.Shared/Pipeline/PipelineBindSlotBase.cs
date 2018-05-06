using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal delegate void PipelineBindSlotDelegate(PipelineBindSlotBase slot, PipelineObjectBase obj);

    internal abstract class PipelineBindSlotBase
    {
        /// <summary>Invoked when the object bound to the slot is disposed.</summary>
        public event PipelineBindSlotDelegate OnObjectForcedUnbind;

        internal PipelineBindSlotBase(int slotID)
        {
            SlotID = slotID;
        }

        internal void BoundObjectDisposed(PipelineObjectBase obj)
        {
            if (obj == Object)
            {
                OnObjectForcedUnbind?.Invoke(this, obj);
                UnbindDisposedObject(obj);
            }
        }

        /// <summary>
        /// Triggers a mandatory unbinding of any object currently bound to the slot.
        /// </summary>
        /// <param name="pipe"></param>
        internal void ForceUnbind()
        {
            OnObjectForcedUnbind?.Invoke(this, Object);
            OnForceUnbind();
        }

        /// <summary>
        /// Occurs when the bound object is disposed.
        /// </summary>
        /// <param name="obj">The object which was just disposed</param>
        protected abstract void UnbindDisposedObject(PipelineObjectBase obj);

        protected abstract void OnForceUnbind();

        internal int SlotID { get; private set; }

        internal abstract PipelineObjectBase Object { get; }

        internal PipelineBindType BindType { get; private protected set; }
    }
}
