using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal class ShaderStateBank<T> : EngineObject
        where T : PipelineObject
    {
        T[] _states;

        internal ShaderStateBank()
        {
            _states = new T[(int)StateConditions.All + 1];
        }

        protected override void OnDispose()
        {
            for (int i = 0; i < _states.Length; i++)
                DisposeObject(ref _states[i]);

            base.OnDispose();
        }

        public T this[StateConditions conditions]
        {
            get => _states[(int)conditions];
            set => _states[(int)conditions] = value;
        }
    }
}
