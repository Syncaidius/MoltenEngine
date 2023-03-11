namespace Molten.Graphics
{
    /// <summary>A base interface for mesh implementations.</summary>
    public abstract class Mesh : Renderable
    {
        IIndexBuffer _iBuffer;

        /// <summary>
        /// Creates a new instance of <see cref="Mesh"/>, but can only be called by derived mesh classes.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="maxVertices"></param>
        /// <param name="mode"></param>
        /// <param name="maxIndices">The maximum number of indices to allow in the current <see cref="Mesh"/>.</param>
        /// <param name="indexFormat">The index format.</param>
        /// <param name="initialIndices"></param>
        protected Mesh(RenderService renderer, BufferMode mode, uint maxVertices, IndexBufferFormat indexFormat, uint maxIndices, Array initialIndices = null) : 
            base(renderer)
        {
            IndexFormat = indexFormat;
            MaxVertices = maxVertices;

            if(indexFormat != IndexBufferFormat.None)
                _iBuffer = Renderer.Device.CreateIndexBuffer(indexFormat, mode, maxIndices, initialIndices);
        }

        public void SetIndices<I>(I[] data) where I : unmanaged
        {
            SetIndices(data, 0, (uint)data.Length);
        }

        public void SetIndices<I>(I[] data, uint count) where I : unmanaged
        {
            SetIndices(data, 0, count);
        }

        public void SetIndices<I>(I[] data, uint startIndex, uint count) where I : unmanaged
        {
            if (_iBuffer == null)
                throw new InvalidOperationException($"Mesh is not indexed. Must be created with index format that isn't IndexBufferFormat.None.");

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
            _iBuffer.Dispose();
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
        public uint VertexCount { get; set; }

        public uint MaxIndices { get; set; }

        public uint IndexCount { get; set; }

        public IndexBufferFormat IndexFormat { get; }

        /// <summary>
        /// Gets or sets the material that should be used when rendering the current <see cref="Mesh"/>.
        /// </summary>
        public HlslShader Shader { get; set; }

        public float EmissivePower { get; set; } = 1.0f;
    }

    public class Mesh<T> : Mesh
        where T : unmanaged, IVertexType
    {
        IVertexBuffer _vb;

        internal Mesh(RenderService renderer, 
            BufferMode mode, uint maxVertices, 
            IndexBufferFormat indexFormat, uint maxIndices,
            T[] initialVertices = null, Array initialIndices = null) :
            base(renderer, mode, maxVertices, indexFormat, maxIndices)
        {
            _vb = renderer.Device.CreateVertexBuffer<T>(mode, maxVertices);
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
            _vb.Dispose();
        }
    }
}
