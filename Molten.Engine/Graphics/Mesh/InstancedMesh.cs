using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.IO;

namespace Molten.Graphics
{
    public delegate void WriteInstanceDataCallback(RawStream stream, RenderCamera camera, ObjectRenderData objData, int index);

    public class InstancedMesh<V, I> : Mesh<V>
        where V : unmanaged, IVertexType
        where I : unmanaged, IVertexType
    {

        IGraphicsBufferSegment _instanceBuffer;
        uint _instanceCount;
        WriteInstanceDataCallback _batchCallback;

        /// <summary>
        /// Creates a new instance of <see cref="InstancedMesh{V, I}"/>.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="maxVertices"></param>
        /// <param name="topology"></param>
        /// <param name="numInstances"></param>
        /// <param name="dynamicVertex"></param>
        /// <param name="batchInstanceCallback">A callback for automatically writing batched draw-instance data.
        /// <para>Setting this to null will prevent automatic batching of the current <see cref="InstancedMesh{V, I}"/></para></param>
        internal InstancedMesh(
            RenderService renderer, 
            uint maxVertices, 
            VertexTopology topology, 
            uint numInstances, 
            bool dynamicVertex,
            WriteInstanceDataCallback batchInstanceCallback = null) : 
            base(renderer, maxVertices, topology, dynamicVertex)
        {
            MaxInstances = numInstances;
            _batchCallback = batchInstanceCallback;

            IGraphicsBuffer buffer = _renderer.DynamicVertexBuffer;
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

        protected override void OnPostDraw(GraphicsCommandQueue cmd)
        {
            base.OnPostDraw(cmd);
            cmd.VertexBuffers[1].Value = null;
        }

        protected override bool OnBatchRender(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, RenderDataBatch batch)
        {
            _instanceCount = (uint)batch.Data.Count;

            if (_instanceCount == 0 || Material == null)
                return true;

            if (_batchCallback != null)
            {
                // TODO Properly handle batches that are larger than the instance buffer.

                uint start = 0;
                uint byteOffset = 0;

                _instanceBuffer.GetStream(GraphicsPriority.Immediate,
                    (buffer, stream) =>
                    {
                        stream.Position += byteOffset;
                        for (int i = (int)start; i < batch.Data.Count; i++)
                            _batchCallback(stream, camera, batch.Data[i], i);
                    },
                    _renderer.StagingBuffer);
            }

            OnApply(cmd);
            ApplyResources(Material);
            cmd.DrawInstanced(Material, VertexCount, _instanceCount, Topology, 0, 0);
            OnPostDraw(cmd);
            
            return true;
        }

        public override void Dispose()
        {
            base.Dispose();
            _instanceBuffer.Release();
        }

        public uint MaxInstances { get; }

        public uint InstanceCount => _instanceCount;
    }
}
