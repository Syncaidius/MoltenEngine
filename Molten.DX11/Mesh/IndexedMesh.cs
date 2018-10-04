using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Represents an indexed mesh. These store mesh data by referring to vertices using index values stored in an index buffer. 
    /// In most cases this reduces the vertex data size drastically.</summary>
    /// <typeparam name="T">The vertex type in the form of a <see cref="IVertexType"/> type.</typeparam>
    /// <seealso cref="Molten.Graphics.Mesh{T}" />
    /// <seealso cref="Molten.Graphics.IIndexedMesh" />
    public class IndexedMesh<T> : Mesh<T>, IIndexedMesh<T> where T : struct, IVertexType
    {
        private protected BufferSegment _ib;
        private protected int _maxIndices;
        private protected IndexBufferFormat _iFormat;
        private protected int _indexCount;

        internal IndexedMesh(RendererDX11 renderer, int maxVertices, int maxIndices, VertexTopology topology, IndexBufferFormat indexFormat, bool dynamic) : 
            base(renderer, maxVertices, topology, dynamic)
        {
            _maxIndices = maxIndices;
            _iFormat = indexFormat;

            GraphicsBuffer iBuffer = dynamic ? renderer.DynamicVertexBuffer : renderer.StaticVertexBuffer;

            switch (_iFormat)
            {
                case IndexBufferFormat.Unsigned16Bit:
                    _ib = iBuffer.Allocate<ushort>(maxIndices);
                    break;

                case IndexBufferFormat.Unsigned32Bit:
                    _ib = iBuffer.Allocate<uint>(maxIndices);
                    break;
            }

            _ib.SetIndexFormat(indexFormat);
        }

        public void SetIndices<I>(I[] data) where I : struct
        {
            SetIndices<I>(data, 0, data.Length);
        }

        public void SetIndices<I>(I[] data, int count) where I : struct
        {
            SetIndices<I>(data, 0, count);
        }

        public void SetIndices<I>(I[] data, int startIndex, int count) where I : struct
        {
            _indexCount = count;
            _ib.SetData(_renderer.Device, data, startIndex, count, 0, _renderer.StagingBuffer); // Staging buffer will be ignored if the mesh is dynamic.
        }

        internal override void ApplyBuffers(GraphicsPipe pipe)
        {
            base.ApplyBuffers(pipe);
            pipe.SetIndexSegment(_ib);
        }

        private protected override void OnRender(GraphicsPipe pipe, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data)
        {
            if (_material == null)
                return;

            ApplyBuffers(pipe);
            ApplyResources(_material);
            _material.Object.Wvp.Value = Matrix4F.Multiply(data.RenderTransform, camera.ViewProjection);

            renderer.Device.DrawIndexed(_material, _indexCount, _topology);
        }

        public int MaxIndices => _maxIndices;

        public int IndexCount => _indexCount;
    }
}
