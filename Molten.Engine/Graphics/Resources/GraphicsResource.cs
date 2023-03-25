using System;
using System.Collections.Generic;
using System.Linq;
using Molten.Collections;

namespace Molten.Graphics
{
    public abstract class GraphicsResource : GraphicsObject
    {
        ThreadedQueue<IGraphicsResourceTask> _applyTaskQueue;

        protected GraphicsResource(GraphicsDevice device, GraphicsBindTypeFlags bindFlags) : 
            base(device, bindFlags)
        {
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
        /// Gets whether the current <see cref="GraphicsResource"/> is unordered-access, meaning it can be written to by certain shaders.
        /// </summary>
        public abstract bool IsUnorderedAccess { get; }

        public abstract uint SizeInBytes { get; }

        public abstract GraphicsResourceFlags Flags { get; }

        internal GraphicsStream Stream { get; set; }
    }
}
