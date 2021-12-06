using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class PipeObject : EngineObject
    {
        internal PipeObject(DeviceDX11 device)
        {
            Device = device;
        }

        protected override sealed void OnDispose()
        {
            Device.MarkForDisposal(this);
        }

        internal abstract void PipelineDispose();


        public DeviceDX11 Device { get; }
    }
}
