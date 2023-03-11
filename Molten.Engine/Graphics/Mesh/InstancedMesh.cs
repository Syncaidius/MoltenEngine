using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.IO;

namespace Molten.Graphics
{
    /// <summary>
    /// A helper version of <see cref="InstancedMesh{V, I}"/> that is defaulted to <see cref="InstanceData"/> for per-instance data.
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public class InstancedMesh<V> : InstancedMesh<V, InstanceData>
        where V : unmanaged, IVertexType
    {
        internal InstancedMesh(RenderService renderer, 
            BufferMode mode, uint maxVertices, 
            IndexBufferFormat indexFormat, uint maxIndices, uint maxInstances, 
            V[] initialVertices = null, Array initialIndices = null, InstanceData[] initialInstances = null) :
            base(renderer, mode, maxVertices, indexFormat, maxIndices, maxInstances, initialVertices, initialIndices, initialInstances)
        {
        }
    }

    public class InstancedMesh<V, I> : Mesh<V>
        where V : unmanaged, IVertexType
        where I : unmanaged, IVertexInstanceType
    {
        IVertexBuffer _instanceBuffer;
        uint _instanceCount;

        /// <summary>
        /// Creates a new instance of <see cref="InstancedMesh{V, I}"/>.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="mode"></param>
        /// <param name="maxVertices"></param>
        /// <param name="indexFormat"></param>
        /// <param name="maxIndices"></param>
        /// <param name="maxInstances"></param>
        /// <param name="initialInstances"></param>
        internal InstancedMesh(RenderService renderer, BufferMode mode, uint maxVertices, IndexBufferFormat indexFormat, uint maxIndices, uint maxInstances,
            V[] initialVertices = null,
            Array initialIndices = null,
            I[] initialInstances = null) : 
            base(renderer, mode, maxVertices, indexFormat, maxIndices, initialVertices, initialIndices)
        {
            MaxInstances = maxInstances;
            _instanceBuffer = Renderer.Device.CreateVertexBuffer(mode, maxIndices, initialInstances);
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
            _instanceBuffer.SetData(GraphicsPriority.Apply, data, startIndex, count, 0, Renderer.StagingBuffer); // Staging buffer will be ignored if the mesh is dynamic.
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

        protected override void OnDraw(GraphicsCommandQueue cmd)
        {
            if (MaxIndices > 0)
                cmd.DrawIndexedInstanced(Shader, IndexCount, _instanceCount);
            else
                cmd.DrawInstanced(Shader, VertexCount, _instanceCount, 0, 0);
        }

        protected override bool OnBatchRender(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, RenderDataBatch batch)
        {
            _instanceCount = (uint)batch.Data.Count;

            if (_instanceCount == 0 || Shader == null)
                return true;

            if (I.IsBatched)
            {
                // TODO Properly handle batches that are larger than the instance buffer.

                uint start = 0;
                uint byteOffset = 0;

                _instanceBuffer.GetStream(GraphicsPriority.Immediate,
                    (buffer, stream) =>
                    {
                        stream.Position += byteOffset;
                        for (int i = (int)start; i < batch.Data.Count; i++)
                            I.WriteBatchData(stream, batch.Data[i]);
                    },
                    Renderer.StagingBuffer);
            }

            Shader.Scene.ViewProjection.Value = camera.ViewProjection;

            OnApply(cmd);
            ApplyResources(Shader);
            OnDraw(cmd);
            OnPostDraw(cmd);
            
            return true;
        }

        public override void Dispose()
        {
            base.Dispose();
            _instanceBuffer.Dispose();
        }

        public uint MaxInstances { get; }

        public uint InstanceCount => _instanceCount;
    }
}
