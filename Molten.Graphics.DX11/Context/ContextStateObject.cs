using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class ContextStateObject : EngineObject
    {
        internal ContextStateObject(Device device)
        {
            Device = device;
        }

        protected override sealed void OnDispose()
        {
            Device.MarkForDisposal(this);
        }

        internal abstract void PipelineDispose();

        /// <summary>
        /// Gets the <see cref="Graphics.Device"/> that the current <see cref="ContextStateObject"/> is bound to.
        /// </summary>
        public Device Device { get; }

        public uint Version { get; protected set; }
    }
}
