namespace Molten.Graphics
{
    public class Mesh<T> : Renderable, IMesh<T> 
        where T : unmanaged, IVertexType
    {
        // private protected is new in C# 7.2. See: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/private-protected
        private protected RendererDX11 _renderer;
        private protected BufferSegment _vb;
        private protected uint _vertexCount;
        private protected bool _isDynamic;

        internal Mesh(RenderService renderer, uint maxVertices, VertexTopology topology, bool dynamic) :
            base(renderer.Device)
        {
            _renderer = renderer as RendererDX11;
            MaxVertices = maxVertices;
            Topology = topology;

            GraphicsBuffer vBuffer = dynamic ? _renderer.DynamicVertexBuffer : _renderer.StaticVertexBuffer;

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
            _vb.SetData(_renderer.NativeDevice.Cmd, data, startIndex, count, 0, _renderer.StagingBuffer); // Staging buffer will be ignored if the mesh is dynamic.
        }

        internal virtual void ApplyBuffers(GraphicsCommandQueue cmd)
        {
            (cmd as CommandQueueDX11).VertexBuffers[0].Value = _vb;
        }

        protected override void OnRender(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data)
        {
            if (Material == null)
                return;

            ApplyBuffers(cmd);
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

        public uint MaxVertices { get; }

        public VertexTopology Topology { get; }

        public Material Material { get; set; }

        public uint VertexCount => _vertexCount;

        public bool IsDynamic => _isDynamic;

        public float EmissivePower { get; set; } = 1.0f;
    }
}
