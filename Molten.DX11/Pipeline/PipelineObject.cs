using SharpDX.Direct3D11;
using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class PipelineObject : EngineObject
    {
        internal ThreadedList<PipelineBindSlot> Binds = new ThreadedList<PipelineBindSlot>();

        internal virtual void Bind(GraphicsPipe pipe, PipelineBindSlot slot)
        {
            Binds.Add(slot);
        }

        internal virtual void Unbind(GraphicsPipe pipe, PipelineBindSlot slot)
        {
            Binds.Remove(slot);
        }

        /// <summary>Invoked when the object is given a chance to refresh while bound to a pipeline slot.</summary>
        /// <param name="pipe">The pipe.</param>
        /// <param name="slot">The slot.</param>
        internal virtual void Refresh(GraphicsPipe pipe, PipelineBindSlot slot) { }

        protected override void OnDispose()
        {
            Binds.ForInterlock(0, 1, (index, slot) =>
            {
                slot.BoundObjectDisposed(this);
                return false;
            });

            Binds.Clear();
            base.OnDispose();
        }
    }
}
