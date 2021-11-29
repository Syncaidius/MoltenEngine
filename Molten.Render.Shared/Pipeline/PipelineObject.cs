using System;
using System.Collections.Generic;

namespace Molten.Graphics
{

    public abstract class PipelineObject<D, P> : PipelineDisposableObject
        where D : IGraphicsDevice
        where P : IGraphicsPipe<D>
    {
        const int BIND_LIST_COUNT = (int)PipelineBindType.Output + 1;
        List<PipelineBindSlot<D, P>>[] _binds;

        internal PipelineObject(D device)
        {
            Device = device;

            _binds = new List<PipelineBindSlot<D, P>>[BIND_LIST_COUNT];
            for (int i = 0; i < _binds.Length; i++)
                _binds[i] = new List<PipelineBindSlot<D, P>>();
        }

        internal void Bind(PipelineBindSlot<D, P> slot)
        {
            _binds[(int)slot.BindType].Add(slot);

            if (slot.BindType == PipelineBindType.Input)
            {
                List<PipelineBindSlot<D, P>> unbinds = _binds[(int)PipelineBindType.Output];
                for (int i = 0; i < unbinds.Count; i++)
                    unbinds[i].ForceUnbind();
            }
            else if (slot.BindType == PipelineBindType.Output)
            {
                List<PipelineBindSlot<D, P>> unbinds = _binds[(int)PipelineBindType.Input];
                for (int i = 0; i < unbinds.Count; i++)
                    unbinds[i].ForceUnbind();
            }
        }

        internal void Unbind(PipelineBindSlot<D, P> slot)
        {
            if (!_binds[(int)slot.BindType].Remove(slot))
                throw new Exception();
        }

        /// <summary>Invoked when the object is given a chance to refresh while bound to a pipeline slot.</summary>
        /// <param name="pipe">The pipe.</param>
        /// <param name="slot">The slot.</param>
        internal virtual void Refresh(P pipe, PipelineBindSlot<D, P> slot) { }


        protected override sealed void OnDispose()
        {
            Device.MarkForDisposal(this);
        }

        /// <summary>
        /// 
        /// </summary>
        internal override sealed void PipelineDispose()
        {
            foreach (List<PipelineBindSlot<D, P>> bindList in _binds)
            {
                for (int j = 0; j < bindList.Count; j++)
                    bindList[j].BoundObjectDisposed(this);

                bindList.Clear();
            }

            base.Dispose(); // Signal bindings about disposal first (via base class).
            OnPipelineDispose();
        }

        private protected abstract void OnPipelineDispose();

        /// <summary>
        /// Gets the device that the object is bound to.
        /// </summary>
        internal D Device { get; private set; }
    }
}
