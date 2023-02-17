namespace Molten.Graphics
{
    /// <summary>A base interface for mesh implementations.</summary>
    public abstract class Mesh : Renderable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="maxVertices"></param>
        /// <param name="topology"></param>
        /// <param name="isDynamic"></param>
        protected Mesh(RenderService renderer, uint maxVertices, VertexTopology topology, bool isDynamic) : base(renderer)
        {
            MaxVertices = maxVertices;
            Topology = topology;
            IsDynamic = isDynamic;
        }

        protected abstract void OnApply(GraphicsCommandQueue cmd);

        protected abstract void OnPostDraw(GraphicsCommandQueue cmd);

        protected abstract void OnDraw(GraphicsCommandQueue cmd);

        protected override sealed void OnRender(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data)
        {
            if (Material == null)
                return;

            OnApply(cmd);
            ApplyResources(Material);
            Material.Object.Wvp.Value = Matrix4F.Multiply(data.RenderTransform, camera.ViewProjection);
            Material.Object.World.Value = data.RenderTransform;
            OnDraw(cmd);
            OnPostDraw(cmd);
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

        /// <summary>Gets the topology/structure of the mesh's data (e.g. line, triangles list/strip, etc).</summary>
        public VertexTopology Topology { get; }

        /// <summary>
        /// Gets or sets the material that should be used when rendering the current <see cref="Mesh"/>.
        /// </summary>
        public Material Material { get; set; }

        public float EmissivePower { get; set; } = 1.0f;
    }

    public class Mesh<T> : Mesh
        where T : unmanaged, IVertexType
    {
        private protected IGraphicsBufferSegment _vb;

        internal Mesh(RenderService renderer, uint maxVertices, VertexTopology topology, bool isDynamic) :
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
            _vb.SetData(data, startIndex, count, 0, Renderer.StagingBuffer); // Staging buffer will be ignored if the mesh is dynamic.
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            cmd.VertexBuffers[0].Value = _vb;
        }

        protected override void OnDraw(GraphicsCommandQueue cmd)
        {
            cmd.Draw(Material, VertexCount, Topology);
        }

        protected override void OnPostDraw(GraphicsCommandQueue cmd)
        {
            cmd.VertexBuffers[0].Value = null;
        }

        public virtual void Dispose()
        {
            IsVisible = false;
            _vb.Release();
        }
    }
}
