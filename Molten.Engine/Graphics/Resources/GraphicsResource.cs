using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Molten.Collections;

namespace Molten.Graphics.Resources
{
    public abstract class GraphicsResource : GraphicsObject
    {

        ThreadedQueue<IGraphicsResourceTask> _pendingChanges;

        protected GraphicsResource(GraphicsDevice device, GraphicsBindTypeFlags bindFlags) : 
            base(device, bindFlags)
        {
            _pendingChanges = new ThreadedQueue<IGraphicsResourceTask>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void QueueOperation(GraphicsPriority priority, IGraphicsResourceTask op)
        {
            if (priority == GraphicsPriority.Immediate)
                op.Process(Device.Cmd);
            else
                _pendingChanges.Enqueue(op);
        }

        /// <summary>Applies any pending changes onto the buffer.</summary>
        /// <param name="context">The graphics pipe to use when process changes.</param>
        /// <param name="forceInitialize">If set to true, the buffer will be initialized if not done so already.</param>
        protected void ApplyChanges(GraphicsCommandQueue context)
        {
            if (_pendingChanges.Count > 0)
            {
                IGraphicsResourceTask op = null;
                while (_pendingChanges.TryDequeue(out op))
                    op.Process(context);
            }
        }

        internal void Clear()
        {
            _pendingChanges.Clear();
        }

        protected override void OnApply(GraphicsCommandQueue context)
        {
            ApplyChanges(context);
        }
    }
}
