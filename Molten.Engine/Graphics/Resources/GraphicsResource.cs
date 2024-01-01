using Molten.Collections;

namespace Molten.Graphics;
{
    public abstract class GraphicsResource : GraphicsObject, IGraphicsResource
    {
        protected GraphicsResource(GraphicsDevice device, GraphicsResourceFlags flags) : 
            base(device)
        {
            Flags = flags;
            ApplyQueue = new ThreadedQueue<IGraphicsResourceTask>();
            LastUsedFrameID = Device.Renderer.FrameID;
        }

        protected virtual void ValidateFlags()
        {
            // Only staging resources have CPU-write access.
            if (Flags.Has(GraphicsResourceFlags.CpuWrite))
            {
                if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
                    throw new GraphicsResourceException(this, "Staging textures cannot allow shader access. Add GraphicsResourceFlags.NoShaderAccess flag.");

                // Staging buffers cannot have any other flags aside from 
                if (Flags != (GraphicsResourceFlags.CpuWrite | GraphicsResourceFlags.CpuRead | GraphicsResourceFlags.None | GraphicsResourceFlags.GpuWrite))
                    throw new GraphicsResourceException(this, "Staging textures must have all CPU/GPU read and write flags.");
            }
        }

        /// <summary>
        /// Gives a derived resource implementation a chance to provide it's maximum supported frame-buffer size.
        /// </summary>
        /// <param name="frameBufferSize">The renderer's frame-buffer size.</param>
        /// <returns></returns>
        protected virtual uint GetMaxFrameBufferSize(uint frameBufferSize)
        {
            return frameBufferSize;
        }

        /// <summary>
        /// Invoked when the next frame has started to give the resource a chance to prepare it's underlying memory or state.
        /// </summary>
        /// <param name="queue">The <see cref="GraphicsQueue"/> that is performing the frame transition.</param>
        /// <param name="frameBufferIndex">The frame buffer or in-flight frame index. This will be between 0 and the back-buffer size.</param>
        /// <param name="frameID">The frame ID. This is based on a total count of frames processed so far.</param>
        protected abstract void OnNextFrame(GraphicsQueue queue, uint frameBufferIndex, ulong frameID);

        protected abstract void OnCreateResource(uint frameBufferSize, uint frameBufferIndex, ulong frameID);

        protected abstract void OnFrameBufferResized(uint lastFrameBufferSize, uint frameBufferSize, uint frameBufferIndex, ulong frameID);

        /// <summary>
        /// Ensure the resource has been initialized.
        /// </summary>
        /// <param name="queue">The queue that will perform initialization, if needed.</param>
        public void Ensure(GraphicsQueue queue)
        {
            uint fbSize = 1;
            uint fbIndex = 0;

            // Cap frame-buffer size to 1 if the resource is static.
            if (Flags.Has(GraphicsResourceFlags.Buffered))
            {
                fbSize = GetMaxFrameBufferSize(Device.FrameBufferSize);
                fbIndex = Math.Min(fbSize - 1, Device.FrameBufferIndex);
            }

            LastUsedFrameID = Device.Renderer.FrameID;

            // Check if the last known frame buffer size has changed.
            if (Handle == null)
            {
                OnCreateResource(fbSize, fbIndex, Device.Renderer.FrameID);
            }
            else if (KnownFrameBufferSize != fbSize)
            {
                OnFrameBufferResized(KnownFrameBufferSize, fbSize, fbIndex, Device.Renderer.FrameID);
                LastFrameResizedID = Device.Renderer.FrameID;
            }
            else if (LastUsedFrameBufferIndex != fbIndex)
            {
                OnNextFrame(queue, Device.FrameBufferIndex, Device.Renderer.FrameID);
                LastUsedFrameBufferIndex = fbIndex;
            }

            KnownFrameBufferSize = fbSize;
        }

        /// <summary>
        /// Invoked when the current <see cref="GraphicsObject"/> should apply any changes before being bound to a GPU context.
        /// </summary>
        /// <param name="queue">The <see cref="GraphicsQueue"/> that the current <see cref="GraphicsObject"/> is to be bound to.</param>
        public void Apply(GraphicsQueue queue)
        {
            if (IsDisposed)
                return;

            Ensure(queue);
            OnApply(queue);
        }

        /// <summary>Applies any pending changes to the resource, from the specified priority queue.</summary>
        /// <param name="cmd">The graphics queue to use when process changes.</param>
        protected virtual void OnApply(GraphicsQueue cmd)
        {
            if (ApplyQueue.Count > 0)
            {
                IGraphicsResourceTask op = null;
                bool altered = false;
                while (ApplyQueue.TryDequeue(out op))
                    altered = op.Process(cmd, this);

                // If the resource was invalided, let the pipeline know it needs to be reapplied by incrementing version.
                if (altered)
                    Version++;
            }
        }

        /// <summary>
        /// Takes the next task from the task queue if it matches the specified type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="result">The task that was dequeued, if any.</param>
        /// <returns></returns>
        protected bool DequeueTaskIfType<T>(out T result)
            where T : IGraphicsResourceTask
        {
            if (ApplyQueue.Count > 0 && ApplyQueue.IsNext<T>())
            {
                if (ApplyQueue.TryDequeue(out IGraphicsResourceTask task))
                {
                    result = (T)task;
                    return true;
                }
            }

            result = default;
            return false;
        }

        public void CopyTo(GraphicsPriority priority, GraphicsResource destination, Action<GraphicsResource> completeCallback = null)
        {
            if (!Flags.Has(GraphicsResourceFlags.GpuRead))
                throw new ResourceCopyException(this, destination, "Source resource must have the GraphicsResourceFlags.GpuRead flag set.");

            if (!destination.Flags.Has(GraphicsResourceFlags.GpuWrite))
                throw new ResourceCopyException(this, destination, "Destination resource must have the GraphicsResourceFlags.GpuWrite flag set.");

            // If copying between two images, do a format and bounds check
            if (this is GraphicsTexture srcTex)
            {
                if (destination is GraphicsTexture destTex)
                {
                    if (ResourceFormat != destination.ResourceFormat)
                        throw new ResourceCopyException(this, destination, "The source and destination texture formats do not match.");

                    // Validate dimensions.
                    if (destTex.Width != srcTex.Width ||
                        destTex.Height != srcTex.Height ||
                        destTex.Depth != srcTex.Depth)
                        throw new ResourceCopyException(this, destination, "The source and destination textures must have the same dimensions.");
                }
                else
                {
                    throw new NotImplementedException("Copying a texture to a non-texture is currently unsupported.");
                }
            }
            else if (this is GraphicsBuffer && destination is GraphicsBuffer)
            {
                if (destination.SizeInBytes < SizeInBytes)
                    throw new GraphicsResourceException(this, "The destination buffer is not large enough.");
            }

            Device.Renderer.PushTask(priority, this, new ResourceCopyTask()
            {
                Destination = destination,
                CompletionCallback = completeCallback,
            });
        }

        /// <summary>
        /// Copies a sub-resource from the current <see cref="GraphicsResource"/> to the sub-resource of the destination <see cref="GraphicsResource"/>.
        /// </summary>
        /// <param name="priority"></param>
        /// <param name="sourceLevel"></param>
        /// <param name="sourceSlice"></param>
        /// <param name="destination"></param>
        /// <param name="destLevel"></param>
        /// <param name="destSlice"></param>
        /// <param name="completeCallback"></param>
        /// <exception cref="ResourceCopyException"></exception>
        public void CopyTo(GraphicsPriority priority,
        uint sourceLevel, uint sourceSlice,
        GraphicsResource destination, uint destLevel, uint destSlice,
        Action<GraphicsResource> completeCallback = null)
        {
            if (!Flags.Has(GraphicsResourceFlags.GpuRead))
                throw new ResourceCopyException(this, destination, "The current texture cannot be copied from because the GraphicsResourceFlags.GpuRead flag was not set.");

            if (!destination.Flags.Has(GraphicsResourceFlags.GpuWrite))
                throw new ResourceCopyException(this, destination, "The destination texture cannot be copied to because the GraphicsResourceFlags.GpuWrite flag was not set.");

            // Validate dimensions.
            // TODO this should only test the source and destination level dimensions, not the textures themselves.
            if (this is GraphicsTexture srcTex)
            {
                if (destination is GraphicsTexture destTex)
                {
                    if (ResourceFormat != destination.ResourceFormat)
                        throw new ResourceCopyException(this, destination, "The source and destination texture formats do not match.");

                    if (destTex.Width != srcTex.Width ||
                        destTex.Height != srcTex.Height ||
                        destTex.Depth != srcTex.Depth)
                        throw new ResourceCopyException(this, destination, "The source and destination textures must have the same dimensions.");

                    if (sourceLevel >= srcTex.MipMapCount)
                        throw new ResourceCopyException(this, destination, "The source mip-map level exceeds the total number of levels in the source texture.");

                    if (sourceSlice >= srcTex.ArraySize)
                        throw new ResourceCopyException(this, destination, "The source array slice exceeds the total number of slices in the source texture.");

                    if (destLevel >= destTex.MipMapCount)
                        throw new ResourceCopyException(this, destination, "The destination mip-map level exceeds the total number of levels in the destination texture.");

                    if (destSlice >= destTex.ArraySize)
                        throw new ResourceCopyException(this, destination, "The destination array slice exceeds the total number of slices in the destination texture.");

                    Device.Renderer.PushTask(priority, this, new SubResourceCopyTask()
                    {
                        SrcRegion = null,
                        SrcSubResource = (sourceSlice * srcTex.MipMapCount) + sourceLevel,
                        DestResource = destination,
                        DestStart = Vector3UI.Zero,
                        DestSubResource = (destSlice * destTex.MipMapCount) + destLevel,
                        CompletionCallback = completeCallback,
                    });
                }
                else
                {
                    throw new NotImplementedException("Copying a texture to a non-texture is currently unsupported.");
                }
            }
        }

        internal void Clear()
        {
            ApplyQueue.Clear();
        }

        /// <summary>
        /// The total size of the resource, in bytes for the current frame.
        /// </summary>
        public abstract uint SizeInBytes { get; protected set; }

        /// <summary>
        /// Gets the resource flags that provided given when the current <see cref="GraphicsResource"/> was created.
        /// </summary>
        public GraphicsResourceFlags Flags { get; protected set; }

        /// <summary>
        /// Gets the underlying native resource handle.
        /// </summary>
        public abstract GraphicsResourceHandle Handle { get; }

        /// <summary>
        /// Gets or [protected] sets the <see cref="GraphicsFormat"/> of the resource.
        /// </summary>
        public abstract GraphicsFormat ResourceFormat { get; protected set; }

        /// <summary>
        /// Gets the last known frame buffer size for the current <see cref="GraphicsResource"/>.
        /// </summary>
        public uint KnownFrameBufferSize { get; protected set; }

        /// <summary>
        /// Gets the ID of the frame that the current <see cref="GraphicsResource"/> was applied.
        /// </summary>
        public ulong LastUsedFrameID { get; private set; }

        /// <summary>
        /// Gets the last frame buffer index that the current <see cref="GraphicsResource"/> was applied.
        /// </summary>
        protected uint LastUsedFrameBufferIndex { get; private set; }

        /// <summary>
        /// Gets the ID of the frame that the current <see cref="GraphicsTexture"/> was resized. 
        /// If the texture was never resized then the frame ID will be the ID of the frame that the texture was created.
        /// </summary>
        public ulong LastFrameResizedID { get; internal set; }

        /// <summary>
        /// Gets the queue of tasks that need to be applied to the current <see cref="GraphicsResource"/> during the next <see cref="Apply(GraphicsQueue)"/> call.
        /// </summary>
        public ThreadedQueue<IGraphicsResourceTask> ApplyQueue { get; }
    }
}
