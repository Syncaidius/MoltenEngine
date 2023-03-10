namespace Molten.Graphics
{
    /// <summary>A base interface for mesh implementations.</summary>
    public abstract class Mesh : Renderable
    {
        IGraphicsBufferSegment _iBuffer;
        IndexBufferFormat _indexFormat;

        /// <summary>
        /// Creates a new instance of <see cref="Mesh"/>, but can only be called by derived mesh classes.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="maxVertices"></param>
        /// <param name="topology"></param>
        /// <param name="isDynamic"></param>
        protected Mesh(RenderService renderer, uint maxVertices, PrimitiveTopology topology, bool isDynamic) : base(renderer)
        {
            _indexFormat = IndexBufferFormat.None;
            MaxVertices = maxVertices;
            IsDynamic = isDynamic;
        }

        /// <summary>
        /// After calling this method, any data that was set via <see cref="SetIndices{I}(I[], uint, uint)"/> or its overloads, will be invalidated and require updating.
        /// <para>An index buffer will be allocated to match the capacity of <paramref name="maxIndices"/> multiplied by either 2 (UINT16) or 4 (UINT32) bytes.</para>
        /// <para>If <see cref="IndexBufferFormat.None"/> is used, any existing index buffer will be released back into the buffer rersource bool for reuse elsewhere.</para>
        /// </summary>
        /// <param name="maxIndices">The maximum number of indices to allow in the current <see cref="Mesh"/>.</param>
        /// <param name="format">The index format.</param>
        public void SetIndexParameters(uint maxIndices, IndexBufferFormat format = IndexBufferFormat.Unsigned32Bit)
        {
            // Don't do anything if there are no parameter changes.
            if (maxIndices == MaxIndices && format == _indexFormat)
                return;

            if (format != IndexBufferFormat.None)
            {
                if (maxIndices < 1)
                    throw new InvalidOperationException("Cannot set maxIndices to less than 1 when a valid index format is provided.");

                bool createBuffer = _iBuffer == null || format != IndexFormat || MaxIndices != maxIndices;

                _iBuffer?.Release();

                if (createBuffer)
                {
                    IGraphicsBuffer iBuffer = IsDynamic ? Renderer.DynamicVertexBuffer : Renderer.StaticVertexBuffer;

                    // MSDN: Accepted formats for index buffer data are UINT16 (DXGI_FORMAT_R16_UINT) and UINT32 (DXGI_FORMAT_R32_UINT).
                    switch (format)
                    {
                        case IndexBufferFormat.Unsigned16Bit:
                            _iBuffer = iBuffer.Allocate<ushort>(maxIndices);
                            break;

                        case IndexBufferFormat.Unsigned32Bit:
                            _iBuffer = iBuffer.Allocate<uint>(maxIndices);
                            break;
                    }

                    _iBuffer.SetIndexFormat(format);
                }

                MaxIndices = maxIndices;
                _indexFormat = format;
            }
            else
            {
                _iBuffer?.Release();
                _iBuffer = null;
                MaxIndices = 0;
            }
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
            if (_iBuffer == null)
                throw new InvalidOperationException($"Cannot set indices without valid index format and capacity. Try calling {nameof(SetIndexParameters)}() first.");

            IndexCount = count;
            _iBuffer.SetData(GraphicsPriority.Apply, data, startIndex, count, 0, Renderer.StagingBuffer); // Staging buffer will be ignored if the mesh is dynamic.
        }

        protected virtual void OnApply(GraphicsCommandQueue cmd)
        {
            cmd.IndexBuffer.Value = _iBuffer;
        }

        protected virtual void OnPostDraw(GraphicsCommandQueue cmd)
        {
            cmd.IndexBuffer.Value = null;
        }

        protected virtual void OnDraw(GraphicsCommandQueue cmd)
        {
            if(_iBuffer != null)
                cmd.DrawIndexed(Shader, IndexCount);
            else
                cmd.Draw(Shader, VertexCount);
        }

        protected override sealed void OnRender(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data)
        {
            if (Shader == null)
                return;

            OnApply(cmd);
            ApplyResources(Shader);
            Shader.Object.Wvp.Value = Matrix4F.Multiply(data.RenderTransform, camera.ViewProjection);
            Shader.Object.World.Value = data.RenderTransform;
            OnDraw(cmd);
            OnPostDraw(cmd);
        }

        public virtual void Dispose()
        {
            IsVisible = false;
            _iBuffer.Release();
        }

        /// <summary>
        /// Gets whether the mesh is dynamic. 
        /// <para>Dynamic meshes are more performant at handling frequent data changes/updates.</para> 
        /// <para>Static meshes are preferred when their data will not change too often.</para>
        /// </summary>
        public bool IsDynamic { get; }

        /// <summary>Gets the maximum number of vertices the mesh can contain.</summary>
        public uint MaxVertices { get; }

        /// <summary>Gets the number of vertices stored in the mesh.</summary>
        public uint VertexCount { get; protected set; }

        public uint MaxIndices { get; private set; }

        public uint IndexCount { get; private set; }

        /// <summary>
        /// Gets or sets the material that should be used when rendering the current <see cref="Mesh"/>.
        /// </summary>
        public HlslShader Shader { get; set; }

        public IndexBufferFormat IndexFormat => _indexFormat;

        public float EmissivePower { get; set; } = 1.0f;
    }

    public class Mesh<T> : Mesh
        where T : unmanaged, IVertexType
    {
        private protected IGraphicsBufferSegment _vb;

        internal Mesh(RenderService renderer, uint maxVertices, PrimitiveTopology topology, bool isDynamic) :
            base(renderer, maxVertices, topology, isDynamic)
        {
            IGraphicsBuffer vBuffer = isDynamic ? Renderer.DynamicVertexBuffer : Renderer.StaticVertexBuffer;

            _vb = vBuffer.Allocate<T>(MaxVertices);
            _vb.SetVertexFormat<T>();
        }

        public void SetVertices(T[] data)
        {
            SetVertices(data, 0, (uint)data.Length);
        }

        public void SetVertices(T[] data, uint count)
        {
            SetVertices(data, 0, count);
        }

        public void SetVertices(T[] data, uint startIndex, uint count)
        {
            VertexCount = count;
            _vb.SetData(GraphicsPriority.Apply, data, startIndex, count, 0, Renderer.StagingBuffer); // Staging buffer will be ignored if the mesh is dynamic.
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            base.OnApply(cmd);
            cmd.VertexBuffers[0].Value = _vb;
        }

        protected override void OnPostDraw(GraphicsCommandQueue cmd)
        {
            base.OnPostDraw(cmd);
            cmd.VertexBuffers[0].Value = null;
        }

        public override void Dispose()
        {
            base.Dispose();
            _vb.Release();
        }
    }
}
