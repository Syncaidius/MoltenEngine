namespace Molten.Graphics
{
    internal delegate void PipelineBindSlotDelegate<D, P>(PipelineBindSlot<D, P> slot, PipelineObject<D, P> obj)
        where D : IGraphicsDevice
        where P : IGraphicsPipe<D>;

    internal abstract class PipelineBindSlot<D, P>
        where D : IGraphicsDevice
        where P : IGraphicsPipe<D>
    {
        /// <summary>Invoked when the object bound to the slot is disposed.</summary>
        public event PipelineBindSlotDelegate<D, P> OnObjectForcedUnbind;

        internal PipelineBindSlot(PipelineComponent<D, P> parent, uint slotID)
        {
            SlotID = slotID;
        }

        internal void BoundObjectDisposed(PipelineObject<D, P> obj)
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
        protected abstract void UnbindDisposedObject(PipelineObject<D, P> obj);

        protected abstract void OnForceUnbind();

        internal uint SlotID { get; private set; }

        internal abstract PipelineObject<D, P> Object { get; }

        internal PipelineBindType BindType { get; private protected set; }

        public PipelineComponent<D, P> Parent { get; private set; }
    }

    internal class PipelineBindSlot<T, D, P> : PipelineBindSlot<D, P>
        where T : PipelineObject<D, P>
        where D : IGraphicsDevice
        where P : IGraphicsPipe<D>
    {
        internal PipelineBindSlot(PipelineComponent<D, P> parent, uint slotID) :
            base(parent, slotID)
        { }

        protected override void UnbindDisposedObject(PipelineObject<D, P> obj)
        {
            BoundObject = null;
        }

        protected override void OnForceUnbind()
        {
            BoundObject?.Unbind(this);
            BoundObject = null;
        }

        /// <summary>Binds a new object to the slot. If null, the existing object (if any) will be unbound.</summary>
        /// <param name="pipe">The <see cref="PipeDX11"/> to use to perform any binding operations..</param>
        /// <param name="obj">The <see cref="PipelineDisposableObject"/> to be bound to the object, or null to clear the existing one.</param>
        /// <returns></returns>
        internal bool Bind(P pipe, T obj, PipelineBindType bindType)
        {
            if (obj != null)
            {
                obj.Refresh(pipe, this);
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
            pipe.Profiler.Current.Bindings++;
            return true;
        }

        internal T BoundObject { get; private set; }

        internal override PipelineObject<D, P> Object => BoundObject;
    }
}
