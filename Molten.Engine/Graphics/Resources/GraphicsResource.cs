using System;
using System.Collections.Generic;
using System.Linq;
using Molten.Collections;

namespace Molten.Graphics
{
    public abstract class GraphicsResource : GraphicsObject
    {
        ThreadedQueue<IGraphicsResourceTask> _applyTaskQueue;

        protected GraphicsResource(GraphicsDevice device, GraphicsResourceFlags flags) : 
            base(device, (flags.Has(GraphicsResourceFlags.UnorderedAccess) ? GraphicsBindTypeFlags.Output : GraphicsBindTypeFlags.None) |
                (flags.Has(GraphicsResourceFlags.NoShaderAccess) ? GraphicsBindTypeFlags.None : GraphicsBindTypeFlags.Input))
        {
            Flags = flags;
            _applyTaskQueue = new ThreadedQueue<IGraphicsResourceTask>();
        }

        /// <summary>
        /// Queues a <see cref="IGraphicsResourceTask"/> on the current <see cref="GraphicsResource"/>.
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="op"></param>
        protected void QueueTask(GraphicsPriority priority, IGraphicsResourceTask op)
        {
            switch (priority)
            {
                default:
                case GraphicsPriority.Immediate:
                    if (op.Process(Device.Cmd, this))
                        Version++;
                    break;

                case GraphicsPriority.Apply:
                    _applyTaskQueue.Enqueue(op);
                    break;

                case GraphicsPriority.StartOfFrame:
                    {
                        RunResourceTask task = RunResourceTask.Get();
                        task.Task = op;
                        task.Resource = this;
                        Device.Renderer.PushTask(RenderTaskPriority.StartOfFrame, task);
                    }
                    break;

                case GraphicsPriority.EndOfFrame:
                    {
                        RunResourceTask task = RunResourceTask.Get();
                        task.Task = op;
                        task.Resource = this;
                        Device.Renderer.PushTask(RenderTaskPriority.EndOfFrame, task);
                    }
                    break;
            }
        }

        /// <summary>Applies any pending changes to the resource, from the specified priority queue.</summary>
        /// <param name="context">The graphics pipe to use when process changes.</param>
        protected void ApplyChanges(GraphicsCommandQueue context)
        {
            if (_applyTaskQueue.Count > 0)
            {
                IGraphicsResourceTask op = null;
                bool invalidated = false;
                while (_applyTaskQueue.TryDequeue(out op))
                    invalidated = op.Process(context, this);

                // If the resource was invalided, let the pipeline know it needs to be reapplied by incrementing version.
                if (invalidated)
                    Version++;
            }
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            ApplyChanges(cmd);
            _applyTaskQueue.Clear();
        }

        internal void Clear()
        {
            _applyTaskQueue.Clear();
        }

        /// <summary>
        /// The total size of the resource, in bytes.
        /// </summary>
        public abstract uint SizeInBytes { get; }

        /// <summary>
        /// Gets the resource flags that provided given when the current <see cref="GraphicsResource"/> was created.
        /// </summary>
        public GraphicsResourceFlags Flags { get; }

        internal GraphicsStream Stream { get; set; }

        /// <summary>
        /// Gets the underlying native resource handle.
        /// </summary>
        public abstract unsafe void* Handle { get; }

        /// <summary>Gets the native shader resource view attached to the object.</summary>
        public abstract unsafe void* SRV { get; }

        /// <summary>Gets the native unordered-acess/storage view attached to the object.</summary>
        public abstract unsafe void* UAV { get; }

        /// <summary>
        /// Gets or [protected] sets the <see cref="GraphicsFormat"/> of the resource.
        /// </summary>
        public abstract GraphicsFormat ResourceFormat { get; protected set; }
    }
}
