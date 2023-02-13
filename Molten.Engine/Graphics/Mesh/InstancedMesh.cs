using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.IO;

namespace Molten.Graphics
{
    public class InstancedMesh<V, I> : Mesh<V>
        where V : unmanaged, IVertexType
        where I : unmanaged, IVertexType
    {
        public delegate void WriteInstanceDataCallback(RawStream stream, ObjectRenderData objData, int index);

        IGraphicsBufferSegment _instanceBuffer;
        uint _instanceCount;
        WriteInstanceDataCallback _instanceWriteCallback;

        /// <summary>
        /// Creates a new instance of <see cref="InstancedMesh{V, I}"/>.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="maxVertices"></param>
        /// <param name="topology"></param>
        /// <param name="numInstances"></param>
        /// <param name="dynamicVertex"></param>
        /// <param name="dynamicInstance"></param>
        /// <param name="batchInstanceCallback">A callback for automatically writing batched draw-instance data.
        /// <para>Setting this to null will prevent automatic batching of the current <see cref="InstancedMesh{V, I}"/></para></param>
        internal InstancedMesh(
            RenderService renderer, 
            uint maxVertices, 
            VertexTopology topology, 
            uint numInstances, 
            bool dynamicVertex,
            bool dynamicInstance,
            Action<RawStream> batchInstanceCallback = null) : 
            base(renderer, maxVertices, topology, dynamicVertex)
        {
            IsDynamicInstance = dynamicInstance;
            MaxInstances = numInstances;

            IGraphicsBuffer buffer = dynamicInstance ? _renderer.DynamicVertexBuffer : _renderer.StaticVertexBuffer;
            _instanceBuffer = buffer.Allocate<I>(numInstances);
            _instanceBuffer.SetVertexFormat<I>();
        }

        public void SetInstanceData(I[] data)
        {
            SetInstanceData(data, 0, (uint)data.Length);
        }

        public void SetInstanceData(I[] data, uint count)
        {
            SetInstanceData(data, 0, count);
        }

        public void SetInstanceData(I[] data, uint startIndex, uint count)
        {
            _instanceCount = count;
            _instanceBuffer.SetData(data, startIndex, count, 0, _renderer.StagingBuffer); // Staging buffer will be ignored if the mesh is dynamic.
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            base.OnApply(cmd);
            cmd.VertexBuffers[1].Value = _instanceBuffer;
        }

        protected override void OnUpdateBatchData(RenderDataBatch batch)
        {
            base.OnUpdateBatchData(batch);
            if (_instanceWriteCallback != null)
            {
                // If instances were only removed from the end of the batch, we don't need to perform a buffer update.
                // Simply decrement the instance count.
                if (batch.HasFlags(RenderBatchDirtyFlags.Removed))
                {
                    if (batch.HasFlags(RenderBatchDirtyFlags.End))
                    {
                        _instanceCount = (uint)batch.Data.Count;
                        return;
                    }
                }

                _instanceBuffer.GetStream(GraphicsPriority.Immediate,
                    (buffer, stream) =>
                    {
                        uint start = 0;

                        // If instances have only been added, only the end of the buffer is dirty.
                        // Skip all unaltered instance data to speed-up updates.
                        if (batch.HasFlags(RenderBatchDirtyFlags.End))
                            start = _instanceCount;

                        for (int i = 0; i < batch.Data.Count; i++)
                            _instanceWriteCallback(stream, batch.Data[i], i);
                    },
                    _renderer.StagingBuffer
                );

                _instanceCount = (uint)batch.Data.Count;
            }
        }

        protected override bool OnBatchRender(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera)
        {
            if (_instanceCount > 0)
                cmd.DrawInstanced(Material, VertexCount, _instanceCount, Topology, 0, 0);
            
            return true;
        }

        public override void Dispose()
        {
            base.Dispose();
            _instanceBuffer.Release();
        }

        public uint MaxInstances { get; }

        public uint InstanceCount => _instanceCount;

        public bool IsDynamicInstance { get; }
    }
}
