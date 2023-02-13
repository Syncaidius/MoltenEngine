namespace Molten.Graphics
{
    /// <summary>A base interface for mesh implementations.</summary>
    public abstract class Mesh : Renderable
    {
        protected Mesh(RenderService renderer) : base(renderer) { }

        /// <summary>Gets whether or not the mesh was created as a dynamic mesh. 
        /// Dynamic meshes are preferable when the mesh's data will be changing at least once or more per frame.</summary>
        public abstract bool IsDynamic { get; }

        /// <summary>Gets the maximum number of vertices the mesh can contain.</summary>
        public abstract uint MaxVertices { get; }

        /// <summary>Gets the number of vertices stored in the mesh.</summary>
        public abstract uint VertexCount { get; }

        /// <summary>Gets the topology/structure of the mesh's data (e.g. line, triangles list/strip, etc).</summary>
        public abstract VertexTopology Topology { get; }

        /// <summary>Gets or sets the material applied to the current mesh.</summary>
        public abstract Material Material { get; set; }

        public float EmissivePower { get; set; } = 1.0f;
    }

    public class Mesh<T> : Mesh
        where T : unmanaged, IVertexType
    {
        private protected RenderService _renderer;
        private protected IGraphicsBufferSegment _vb;
        private protected uint _vertexCount;
        private protected bool _isDynamic;

        internal Mesh(RenderService renderer, uint maxVertices, VertexTopology topology, bool dynamic) :
            base(renderer)
        {
            _renderer = renderer;
            MaxVertices = maxVertices;
            Topology = topology;

            IGraphicsBuffer vBuffer = dynamic ? _renderer.DynamicVertexBuffer : _renderer.StaticVertexBuffer;

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
            _vertexCount = count;
            _vb.SetData(data, startIndex, count, 0, _renderer.StagingBuffer); // Staging buffer will be ignored if the mesh is dynamic.
        }

        protected override void OnRender(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data)
        {
            if (Material == null)
                return;

            cmd.VertexBuffers[0].Value = _vb;

            ApplyResources(Material);
            Material.Object.Wvp.Value = Matrix4F.Multiply(data.RenderTransform, camera.ViewProjection);
            Material.Object.World.Value = data.RenderTransform;

            cmd.Draw(Material, _vertexCount, Topology);

            /* TODO: According to: https://www.gamedev.net/forums/topic/667328-vertices-and-indices-in-the-same-buffer/
            *  - A buffer can be bound as both a vertex and index buffer
            *  - If offsets and formats for each segment are correct, a single buffer can be bound at multiple pipeline stages.
            */
        }

        public virtual void Dispose()
        {
            IsVisible = false;
            _vb.Release();
        }

        /// <summary>
        /// Gets the maximum number of vertices that the mesh can contain.
        /// </summary>
        public override uint MaxVertices { get; }

        /// <summary>
        /// Gets the vertex topology of the current <see cref="Mesh"/>.
        /// </summary>
        public override VertexTopology Topology { get; }

        /// <summary>
        /// Gets or sets the material that should be used when rendering the current <see cref="Mesh"/>.
        /// </summary>
        public override Material Material { get; set; }

        /// <summary>
        /// Gets the number of vertices currently in the mesh.
        /// </summary>
        public override uint VertexCount => _vertexCount;

        /// <summary>
        /// Gets whether the mesh is dynamic. 
        /// <para>Dynamic meshes are more performant at handling frequent data changes/updates.</para> 
        /// <para>Static meshes are preferred when their data will not change too often.</para>
        /// </summary>
        public override bool IsDynamic => _isDynamic;
    }
}
