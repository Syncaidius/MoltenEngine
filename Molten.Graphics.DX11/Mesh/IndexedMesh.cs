namespace Molten.Graphics
{
    /// <summary>Represents an indexed mesh. These store mesh data by referring to vertices using index values stored in an index buffer. 
    /// In most cases this reduces the vertex data size drastically.</summary>
    /// <typeparam name="T">The vertex type in the form of a <see cref="IVertexType"/> type.</typeparam>
    /// <seealso cref="Molten.Graphics.Mesh{T}" />
    /// <seealso cref="Molten.Graphics.IIndexedMesh" />
    public class IndexedMesh<T> : Mesh<T>, IIndexedMesh<T> where T : unmanaged, IVertexType
    {
        private protected BufferSegment _ib;
        private protected IndexBufferFormat _iFormat;
        private protected uint _maxIndices;
        private protected uint _indexCount;

        internal IndexedMesh(RendererDX11 renderer, uint maxVertices, uint maxIndices, VertexTopology topology, IndexBufferFormat indexFormat, bool dynamic) : 
            base(renderer, maxVertices, topology, dynamic)
        {
            _maxIndices = maxIndices;
            _iFormat = indexFormat;

            GraphicsBuffer iBuffer = dynamic ? renderer.DynamicVertexBuffer : renderer.StaticVertexBuffer;

            /* MSDN: The only formats allowed for index buffer data are 16-bit (DXGI_FORMAT_R16_UINT) and 32-bit (DXGI_FORMAT_R32_UINT) integers.
             */
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

        public void SetIndices<I>(I[] data) where I : unmanaged
        {
            SetIndices<I>(data, 0, (uint)data.Length);
        }

        public void SetIndices<I>(I[] data, uint count) where I : unmanaged
        {
            SetIndices<I>(data, 0, count);
        }

        public void SetIndices<I>(I[] data, uint startIndex, uint count) where I : unmanaged
        {
            _indexCount = count;
            _ib.SetData(_renderer.NativeDevice.Cmd, data, startIndex, count, 0, _renderer.StagingBuffer); // Staging buffer will be ignored if the mesh is dynamic.
        }

        internal override void ApplyBuffers(CommandQueueDX11 context)
        {
            base.ApplyBuffers(context);
            context.IndexBuffer.Value = _ib;
        }

        private protected override void OnRender(CommandQueueDX11 context, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data)
        {
            if (Material == null)
                return;

            ApplyBuffers(context);
            ApplyResources(Material);
            Material.Object.Wvp.Value = Matrix4F.Multiply(data.RenderTransform, camera.ViewProjection);

            renderer.NativeDevice.Cmd.DrawIndexed(Material, _indexCount, Topology);
        }

        public uint MaxIndices { get; }

        public uint IndexCount { get; }
    }
}
