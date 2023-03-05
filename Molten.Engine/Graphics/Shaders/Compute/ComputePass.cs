using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class ComputePass : HlslPass
    {
        public unsafe void* InputByteCode;

        protected ComputePass(ComputeTask task, string name) : 
            base(task)
        {
            Name = name;
            Composition = new ShaderComposition(this, ShaderType.Compute);
        }

        internal void InitializeState(ComputeStatePreset preset)
        {
            ComputeStateParameters p = new ComputeStateParameters(preset);
            InitializeState(ref p);
        }

        internal void InitializeState(ref ComputeStateParameters parameters)
        {
            if (IsInitialized)
                return;

            OnInitializeState(ref parameters);
            IsInitialized = true;
        }

        protected abstract void OnInitializeState(ref ComputeStateParameters parameters);

        public override void GraphicsRelease()
        {
            Composition.Dispose();
        }

        public ShaderComposition Composition { get; }
    }
}
