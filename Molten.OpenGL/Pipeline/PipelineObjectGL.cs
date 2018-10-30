using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class PipelineObjectGL : PipelineObjectBase
    {
        internal PipelineObjectGL(GraphicsDeviceGL device)
        {
            Device = device;
        }

        /// <summary>Invoked when the object is given a chance to refresh while bound to a pipeline slot.</summary>
        /// <param name="pipe">The pipe.</param>
        /// <param name="slot">The slot.</param>
        internal virtual void Refresh(GraphicsDeviceGL device, PipelineBindSlotGL slot) { }

        /// <summary>
        /// Gets the device that they pipeline object is bound to.
        /// </summary>
        internal GraphicsDeviceGL Device { get; }
    }
}
