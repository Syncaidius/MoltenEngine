namespace Molten.Graphics
{
    public abstract partial class GraphicsDevice
    {
        /// <summary>
        /// Creates a standard mesh. Standard meshes enforce stricter rules aimed at deferred rendering.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="maxVertices"></param>
        /// <param name="maxIndices"></param>
        /// <param name="initialVertices"></param>
        /// <param name="initialIndices"></param>
        /// <returns></returns>
        public Mesh<GBufferVertex> CreateMesh(GraphicsResourceFlags mode, ushort maxVertices, uint maxIndices, GBufferVertex[] initialVertices, ushort[] initialIndices)
        {
            return new StandardMesh(Renderer, mode, maxVertices, maxIndices, initialVertices, initialIndices);
        }

        /// <summary>
        /// Creates a standard mesh. Standard meshes enforce stricter rules aimed at deferred rendering.
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="maxVertices"></param>
        /// <param name="maxIndices"></param>
        /// <param name="initialVertices"></param>
        /// <param name="initialIndices"></param>
        /// <returns></returns>
        public Mesh<GBufferVertex> CreateMesh(GraphicsResourceFlags mode, uint maxVertices, uint maxIndices = 0, GBufferVertex[] initialVertices = null, uint[] initialIndices = null)
        {
            return new StandardMesh(Renderer, mode, maxVertices, maxIndices, initialVertices, initialIndices);
        }

        public Mesh<T> CreateMesh<T>(T[] vertices, ushort[] indices, GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite)
            where T : unmanaged, IVertexType
        {
            if (vertices == null)
                throw new ArgumentNullException($"Vertices array cannot be nulled");

            if (vertices.Length >= ushort.MaxValue)
                throw new NotSupportedException($"The maximum number of vertices is {ushort.MaxValue} when using 16-bit indexing values");

            uint indexCount = indices != null ? (uint)indices.Length : 0;
            return CreateMesh(flags, (ushort)vertices.Length, indexCount, vertices, indices);
        }

        public Mesh<T> CreateMesh<T>(T[] vertices, uint[] indices = null, GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite)
    where T : unmanaged, IVertexType
        {
            if (vertices == null)
                throw new ArgumentNullException($"Vertices array cannot be nulled");

            uint indexCount = indices != null ? (uint)indices.Length : 0;
            return CreateMesh(flags, (uint)vertices.Length, indexCount, vertices, indices);
        }

        /// <summary>
        /// Creates a new mesh. Index data is optional, but can potentially lead to less data transfer when copying to/from the GPU.
        /// </summary>
        /// <typeparam name="T">The type of vertex data.</typeparam>
        /// <param name="mode"></param>
        /// <param name="maxVertices"></param>
        /// <param name="maxIndices"></param>
        /// <param name="initialVertices"></param>
        /// <param name="initialIndices"></param>
        /// <returns></returns>
        public Mesh<T> CreateMesh<T>(GraphicsResourceFlags mode, ushort maxVertices, uint maxIndices, T[] initialVertices, ushort[] initialIndices)
            where T : unmanaged, IVertexType
        {
            return new Mesh<T>(Renderer, mode, maxVertices, maxIndices, initialVertices, initialIndices);
        }

        /// <summary>
        /// Creates a new mesh. Index data is optional, but can potentially lead to less data transfer when copying to/from the GPU.
        /// </summary>
        /// <typeparam name="T">The type of vertex data.</typeparam>
        /// <param name="mode"></param>
        /// <param name="maxVertices"></param>
        /// <param name="maxIndices"></param>
        /// <param name="initialVertices"></param>
        /// <param name="initialIndices"></param>
        /// <returns></returns>
        public Mesh<T> CreateMesh<T>(GraphicsResourceFlags mode, uint maxVertices, uint maxIndices = 0, T[] initialVertices = null, uint[] initialIndices = null)
            where T : unmanaged, IVertexType
        {
            return new Mesh<T>(Renderer, mode, maxVertices, maxIndices, initialVertices, initialIndices);
        }

        public InstancedMesh<V, I> CreateInstancedMesh<V, I>(V[] vertices, uint maxInstances, ushort[] indices, GraphicsResourceFlags flags = GraphicsResourceFlags.None)
            where V : unmanaged, IVertexType
            where I : unmanaged, IVertexInstanceType
        {
            if (vertices.Length >= ushort.MaxValue)
                throw new NotSupportedException($"The maximum number of vertices is {ushort.MaxValue} when using 16-bit indexing values");

            uint maxIndices = indices != null ? (uint)indices.Length : 0;
            return new InstancedMesh<V, I>(Renderer, flags, (ushort)vertices.Length, maxIndices, maxInstances, vertices, indices);
        }

        public InstancedMesh<V, I> CreateInstancedMesh<V, I>(V[] vertices, uint maxInstances, uint[] indices = null, GraphicsResourceFlags flags = GraphicsResourceFlags.None)
            where V : unmanaged, IVertexType
            where I : unmanaged, IVertexInstanceType
        {
            uint maxIndices = indices != null ? (uint)indices.Length : 0;
            return new InstancedMesh<V, I>(Renderer, flags, (ushort)vertices.Length, maxIndices, maxInstances, vertices, indices);
        }

        /// <summary>
        /// Creates a instanced mesh.
        /// </summary>
        /// <typeparam name="V">The type of vertex data.</typeparam>
        /// <typeparam name="I">The type if instance data.</typeparam>
        /// <param name="mode"></param>
        /// <param name="maxVertices"></param>
        /// <param name="maxInstances"></param>
        /// <param name="maxIndices"></param>
        /// <param name="initialVertices"></param>
        /// <param name="initialIndices"></param>
        /// <returns></returns>
        public InstancedMesh<V, I> CreateInstancedMesh<V, I>(GraphicsResourceFlags mode, ushort maxVertices, uint maxInstances, uint maxIndices, V[] initialVertices, ushort[] initialIndices)
            where V : unmanaged, IVertexType
            where I : unmanaged, IVertexInstanceType
        {
            if (initialVertices != null && initialVertices.Length >= ushort.MaxValue)
                throw new NotSupportedException($"The maximum number of vertices is {ushort.MaxValue} when using 16-bit indexing values");

            return new InstancedMesh<V, I>(Renderer, mode, maxVertices, maxIndices, maxInstances, initialVertices, initialIndices);
        }

        /// <summary>
        /// Creates a instanced mesh.
        /// </summary>
        /// <typeparam name="V">The type of vertex data.</typeparam>
        /// <typeparam name="I">The type if instance data.</typeparam>
        /// <param name="mode"></param>
        /// <param name="maxVertices"></param>
        /// <param name="maxInstances"></param>
        /// <param name="maxIndices"></param>
        /// <param name="initialVertices"></param>
        /// <param name="initialIndices"></param>
        /// <returns></returns>
        public InstancedMesh<V, I> CreateInstancedMesh<V, I>(GraphicsResourceFlags mode, uint maxVertices, uint maxInstances, uint maxIndices = 0,
            V[] initialVertices = null, uint[] initialIndices = null)
            where V : unmanaged, IVertexType
            where I : unmanaged, IVertexInstanceType
        {
            return new InstancedMesh<V, I>(Renderer, mode, maxVertices, maxIndices, maxInstances, initialVertices, initialIndices);
        }
    }
}
