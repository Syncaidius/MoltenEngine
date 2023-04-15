namespace Molten.Graphics
{
    /// <summary>A base interface for mesh implementations.</summary>
    public abstract class Mesh : Renderable
    {
        GraphicsBuffer _iBuffer;

        /// <summary>
        /// Creates a new instance of <see cref="Mesh"/>, but can only be called by derived mesh classes.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="maxVertices"></param>
        /// <param name="mode"></param>
        /// <param name="maxIndices">The maximum number of indices to allow in the current <see cref="Mesh"/>.</param>
        /// <param name="initialIndices"></param>
        protected Mesh(RenderService renderer, GraphicsResourceFlags mode, ushort maxVertices, uint maxIndices, ushort[] initialIndices = null) :
            base(renderer)
        {
            IndexFormat = maxIndices > 0 ? IndexBufferFormat.UInt16 : IndexBufferFormat.None;
            MaxVertices = maxVertices;
            IsDiscard = mode.IsDiscard();

            if (IndexFormat != IndexBufferFormat.None)
            {
                _iBuffer = Renderer.Device.CreateIndexBuffer(mode, maxIndices, initialIndices);

                if (initialIndices != null)
                    IndexCount = (uint)initialIndices.Length;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="Mesh"/>, but can only be called by derived mesh classes.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="maxVertices"></param>
        /// <param name="mode"></param>
        /// <param name="maxIndices">The maximum number of indices to allow in the current <see cref="Mesh"/>.</param>
        /// <param name="initialIndices"></param>
        protected Mesh(RenderService renderer, GraphicsResourceFlags mode, uint maxVertices, uint maxIndices, uint[] initialIndices = null) :
            base(renderer)
        {
            IndexFormat = maxIndices > 0 ? IndexBufferFormat.UInt32 : IndexBufferFormat.None;
            MaxVertices = maxVertices;
            MaxIndices = maxIndices;

            if (IndexFormat != IndexBufferFormat.None)
            {
                _iBuffer = Renderer.Device.CreateIndexBuffer(mode, maxIndices, initialIndices);

                if (initialIndices != null)
                    IndexCount = (uint)initialIndices.Length;
            }
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
            _iBuffer.SetData(GraphicsPriority.Apply, data, startIndex, count, IsDiscard, 0, Renderer.Frame.StagingBuffer); // Staging buffer will be ignored if the mesh is dynamic.
        }

        protected virtual void OnApply(GraphicsQueue cmd)
        {
            cmd.IndexBuffer.Value = _iBuffer;
        }

        protected virtual void OnPostDraw(GraphicsQueue cmd)
        {
            cmd.IndexBuffer.Value = null;
        }

        protected virtual void OnDraw(GraphicsQueue cmd)
        {
            if(_iBuffer != null)
                cmd.DrawIndexed(Shader, IndexCount);
            else
                cmd.Draw(Shader, VertexCount);
        }

        protected override sealed void OnRender(GraphicsQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data)
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

        protected bool IsDiscard { get; }
    }

    public class Mesh<T> : Mesh
        where T : unmanaged, IVertexType
    {
        GraphicsBuffer _vb;

        internal Mesh(RenderService renderer, 
            GraphicsResourceFlags mode, ushort maxVertices, uint maxIndices,
            T[] initialVertices = null, ushort[] initialIndices = null) :
            base(renderer, mode, maxVertices, maxIndices, initialIndices)
        {
            _vb = renderer.Device.CreateVertexBuffer(mode, maxVertices, initialVertices);

            if (initialVertices != null)
                VertexCount = (uint)initialVertices.Length;
        }

        internal Mesh(RenderService renderer,
             GraphicsResourceFlags mode, uint maxVertices, uint maxIndices,
             T[] initialVertices = null, uint[] initialIndices = null) :
             base(renderer, mode, maxVertices, maxIndices, initialIndices)
        {
            _vb = renderer.Device.CreateVertexBuffer(mode, maxVertices, initialVertices); 
            
            if (initialVertices != null)
                VertexCount = (uint)initialVertices.Length;
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
            _vb.SetData(GraphicsPriority.Apply, data, startIndex, count, IsDiscard, 0, Renderer.Frame.StagingBuffer); // Staging buffer will be ignored if the mesh is dynamic.
        }

        protected override void OnApply(GraphicsQueue cmd)
        {
            base.OnApply(cmd);
            cmd.VertexBuffers[0].Value = _vb;
        }

        protected override void OnPostDraw(GraphicsQueue cmd)
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
