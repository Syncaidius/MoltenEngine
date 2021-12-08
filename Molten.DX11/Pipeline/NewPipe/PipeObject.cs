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

        /// <summary>
        /// Gets the <see cref="DeviceDX11"/> that the current <see cref="PipeObject"/> is bound to.
        /// </summary>
        public DeviceDX11 Device { get; }
    }
}
