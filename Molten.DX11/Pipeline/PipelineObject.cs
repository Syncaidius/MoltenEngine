using SharpDX.Direct3D11;
using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class PipelineObject : PipelineObjectBase
    {
        internal PipelineObject(GraphicsDeviceDX11 device)
        {
            Device = device;
        }

        /// <summary>Invoked when the object is given a chance to refresh while bound to a pipeline slot.</summary>
        /// <param name="pipe">The pipe.</param>
        /// <param name="slot">The slot.</param>
        internal virtual void Refresh(GraphicsPipe pipe, PipelineBindSlot slot) { }

        protected override sealed void OnDispose()
        {
            Device.MarkForDisposal(this);
        }

        /// <summary>
        /// 
        /// </summary>
        internal void PipelineDispose()
        {
            base.OnDispose(); // Signal bindings about disposal first (via base class).
            OnPipelineDispose();
        }

        private protected abstract void OnPipelineDispose();

        /// <summary>
        /// Gets the <see cref="GraphicsDeviceDX11"/> that the object is bound to.
        /// </summary>
        internal GraphicsDeviceDX11 Device { get; private set; }
    }
}
