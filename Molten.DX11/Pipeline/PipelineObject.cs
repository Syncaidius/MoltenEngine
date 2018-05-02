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
        const int BIND_LIST_COUNT = (int)PipelineBindType.Output + 1;
        List<PipelineBindSlot>[] _binds;

        public PipelineObject()
        {
            _binds = new List<PipelineBindSlot>[BIND_LIST_COUNT];
            for (int i = 0; i < _binds.Length; i++)
                _binds[i] = new List<PipelineBindSlot>();
        }

        internal void Bind(GraphicsPipe pipe, PipelineBindSlot slot)
        {
            _binds[(int)slot.BindType].Add(slot);

            if (slot.BindType == PipelineBindType.Input)
            {
                List<PipelineBindSlot> unbinds = _binds[(int)PipelineBindType.Output];
                for (int i = 0; i < unbinds.Count; i++)
                    unbinds[i].ForceUnbind(pipe);
            }
            else if (slot.BindType == PipelineBindType.Output)
            {
                List<PipelineBindSlot> unbinds = _binds[(int)PipelineBindType.Input];
                for (int i = 0; i < unbinds.Count; i++)
                    unbinds[i].ForceUnbind(pipe);
            }
        }

        internal void Unbind(GraphicsPipe pipe, PipelineBindSlot slot)
        {
            if (!_binds[(int)slot.BindType].Remove(slot))
                throw new Exception();
        }

        /// <summary>Invoked when the object is given a chance to refresh while bound to a pipeline slot.</summary>
        /// <param name="pipe">The pipe.</param>
        /// <param name="slot">The slot.</param>
        internal virtual void Refresh(GraphicsPipe pipe, PipelineBindSlot slot) { }

        protected override void OnDispose()
        {
            foreach (List<PipelineBindSlot> bindList in _binds)
            {
                for (int j = 0; j < bindList.Count; j++)
                    bindList[j].BoundObjectDisposed(this);

                bindList.Clear();
            }
            base.OnDispose();
        }
    }
}
