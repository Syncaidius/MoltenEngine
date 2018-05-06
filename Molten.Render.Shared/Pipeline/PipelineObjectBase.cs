using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class PipelineObjectBase : EngineObject
    {
        const int BIND_LIST_COUNT = (int)PipelineBindType.Output + 1;
        List<PipelineBindSlotBase>[] _binds;

        public PipelineObjectBase()
        {
            _binds = new List<PipelineBindSlotBase>[BIND_LIST_COUNT];
            for (int i = 0; i < _binds.Length; i++)
                _binds[i] = new List<PipelineBindSlotBase>();
        }

        internal void Bind(PipelineBindSlotBase slot)
        {
            _binds[(int)slot.BindType].Add(slot);

            if (slot.BindType == PipelineBindType.Input)
            {
                List<PipelineBindSlotBase> unbinds = _binds[(int)PipelineBindType.Output];
                for (int i = 0; i < unbinds.Count; i++)
                    unbinds[i].ForceUnbind();
            }
            else if (slot.BindType == PipelineBindType.Output)
            {
                List<PipelineBindSlotBase> unbinds = _binds[(int)PipelineBindType.Input];
                for (int i = 0; i < unbinds.Count; i++)
                    unbinds[i].ForceUnbind();
            }
        }

        internal void Unbind(PipelineBindSlotBase slot)
        {
            if (!_binds[(int)slot.BindType].Remove(slot))
                throw new Exception();
        }

        protected override void OnDispose()
        {
            foreach (List<PipelineBindSlotBase> bindList in _binds)
            {
                for (int j = 0; j < bindList.Count; j++)
                    bindList[j].BoundObjectDisposed(this);

                bindList.Clear();
            }
            base.OnDispose();
        }
    }
}
