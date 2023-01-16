using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class GraphicsCommandQueue : EngineObject
    {
        protected class BatchDrawInfo
        {
            public bool Began;
            public StateConditions Conditions;

            public void Reset()
            {
                Began = false;
                Conditions = StateConditions.None;
            }
        }

        RenderProfiler _profiler;
        RenderProfiler _defaultProfiler;

        protected GraphicsCommandQueue(GraphicsDevice device)
        {
            DrawInfo = new BatchDrawInfo();
            Device = device;
            _defaultProfiler = _profiler = new RenderProfiler();
        }

        public void BeginDraw(StateConditions conditions)
        {
#if DEBUG
            if (DrawInfo.Began)
                throw new GraphicsCommandQueueException(this, $"{nameof(GraphicsCommandQueue)}: EndDraw() must be called before the next BeginDraw() call.");
#endif

            DrawInfo.Began = true;
            DrawInfo.Conditions = conditions;
        }

        public void EndDraw()
        {
#if DEBUG
            if (!DrawInfo.Began)
                throw new GraphicsCommandQueueException(this, $"{nameof(GraphicsCommandQueue)}: BeginDraw() must be called before EndDraw().");
#endif

            DrawInfo.Reset();
        }

        /// <summary>Draw non-indexed, non-instanced primitives. 
        /// All queued compute shader dispatch requests are also processed</summary>
        /// <param name="material">The <see cref="IMaterial"/> to apply when drawing.</param>
        /// <param name="vertexCount">The number of vertices to draw from the provided vertex buffer(s).</param>
        /// <param name="vertexStartIndex">The vertex to start drawing from.</param>
        /// <param name="topology">The primitive topology to use when drawing with a NULL vertex buffer. 
        /// Vertex buffers always override this when applied.</param>
        public abstract GraphicsBindResult Draw(IMaterial material, uint vertexCount, VertexTopology topology, uint vertexStartIndex = 0);

        /// <summary>Draw instanced, unindexed primitives. </summary>
        /// <param name="material">The <see cref="IMaterial"/> to apply when drawing.</param>
        /// <param name="vertexCountPerInstance">The expected number of vertices per instance.</param>
        /// <param name="instanceCount">The expected number of instances.</param>
        /// <param name="topology">The expected topology of the indexed vertex data.</param>
        /// <param name="vertexStartIndex">The index of the first vertex.</param>
        /// <param name="instanceStartIndex">The index of the first instance element</param>
        public abstract GraphicsBindResult DrawInstanced(IMaterial material,
            uint vertexCountPerInstance,
            uint instanceCount,
            VertexTopology topology,
            uint vertexStartIndex = 0,
            uint instanceStartIndex = 0);

        /// <summary>Draw indexed, non-instanced primitives.</summary>
        /// <param name="material">The <see cref="IMaterial"/> to apply when drawing.</param>
        /// <param name="vertexIndexOffset">A value added to each index before reading from the vertex buffer.</param>
        /// <param name="indexCount">The number of indices to be drawn.</param>
        /// <param name="startIndex">The index to start drawing from.</param>
        /// <param name="topology">The toplogy to apply when drawing with a NULL vertex buffer. Vertex buffers always override this when applied.</param>
        public abstract GraphicsBindResult DrawIndexed(IMaterial material,
            uint indexCount,
            VertexTopology topology,
            int vertexIndexOffset = 0,
            uint startIndex = 0);

        /// <summary>Draw indexed, instanced primitives.</summary>
        /// <param name="material">The <see cref="IMaterial"/> to apply when drawing.</param>
        /// <param name="indexCountPerInstance">The expected number of indices per instance.</param>
        /// <param name="instanceCount">The expected number of instances.</param>
        /// <param name="topology">The expected topology of the indexed vertex data.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="vertexIndexOffset">The index of the first vertex.</param>
        /// <param name="instanceStartIndex">The index of the first instance element</param>
        public abstract GraphicsBindResult DrawIndexedInstanced(IMaterial material,
            uint indexCountPerInstance,
            uint instanceCount,
            VertexTopology topology,
            uint startIndex = 0,
            int vertexIndexOffset = 0,
            uint instanceStartIndex = 0);

        /// <summary>
        /// Queues a <see cref="IComputeTask"/> for execution, at the descretion of the device it is executed on.
        /// </summary>
        /// <param name="task">The task to be dispatched.</param>
        /// <param name="groupsX">The X thread-group dimension.</param>
        /// <param name="groupsY">The Y thread-group dimension.</param>
        /// <param name="groupsZ">The Z thread-group dimension.</param>
        public abstract void Dispatch(IComputeTask task, uint groupsX, uint groupsY, uint groupsZ);

        protected BatchDrawInfo DrawInfo { get; }

        /// <summary>
        /// Gets the parent <see cref="GraphicsDevice"/> of the current <see cref="GraphicsCommandQueue"/>.
        /// </summary>
        public GraphicsDevice Device { get; }

        /// <summary>Gets the profiler bound to the current <see cref="GraphicsCommandQueue"/>. Contains statistics for this context alone.</summary>
        public RenderProfiler Profiler
        {
            get => _profiler;
            set => _profiler = value ?? _defaultProfiler;
        }
    }
}
