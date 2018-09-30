using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class PipelineBindSlotGL : PipelineBindSlotBase
    {
        internal PipelineBindSlotGL(PipelineComponentGL parent, int slotID) : base(slotID) { }
    }

    internal class PipelineBindSlotGL<T> : PipelineBindSlotGL where T : PipelineObjectGL
    {
        internal PipelineBindSlotGL(PipelineComponentGL parent, int slotID) :
            base(parent, slotID)
        { }

        protected override void UnbindDisposedObject(PipelineObjectBase obj)
        {
            BoundObject = null;
        }

        protected override void OnForceUnbind()
        {
            BoundObject?.Unbind(this);
            BoundObject = null;
        }

        /// <summary>Binds a new object to the slot. If null, the existing object (if any) will be unbound.</summary>
        /// <param name="device">The <see cref="GraphicsPipe"/> to use to perform any binding operations..</param>
        /// <param name="obj">The <see cref="PipelineObject"/> to be bound to the object, or null to clear the existing one.</param>
        /// <returns></returns>
        internal bool Bind(GraphicsDeviceGL device, T obj, PipelineBindType bindType)
        {
            if(obj != null)
            {
                obj.Refresh(device, this);
                if (BoundObject == obj && BindType == bindType)
                    return false;

                BoundObject?.Unbind(this);
                BoundObject = obj;
                BindType = bindType;
                obj.Bind(this);
            }
            else
            {
                if (BoundObject == null && BindType == bindType)
                    return false;

                BoundObject?.Unbind(this);
                BoundObject = obj;
                BindType = bindType;
            }            

            // Return true to signal a difference between old and new object.
            device.Profiler.Current.Bindings++;
            return true;
        }

        internal T BoundObject { get; private set; }

        internal override PipelineObjectBase Object => BoundObject;
    }
}
