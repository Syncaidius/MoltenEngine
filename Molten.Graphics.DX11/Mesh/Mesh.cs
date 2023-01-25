namespace Molten.Graphics
{
    public class Mesh<T> : Renderable, IMesh<T> 
        where T : unmanaged, IVertexType
    {
        // private protected is new in C# 7.2. See: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/private-protected
        private protected RendererDX11 _renderer;
        private protected BufferSegment _vb;
        private protected Material _material;
        private protected uint _vertexCount;
        private protected bool _isDynamic;

        internal Mesh(RendererDX11 renderer, uint maxVertices, VertexTopology topology, bool dynamic) :
            base(renderer.Device)
        {
            _renderer = renderer;
            MaxVertices = maxVertices;
            Topology = topology;

            GraphicsBuffer vBuffer = dynamic ? renderer.DynamicVertexBuffer : renderer.StaticVertexBuffer;

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

        internal virtual void ApplyBuffers(CommandQueueDX11 cmd)
        {
            cmd.VertexBuffers[0].Value = _vb;
        }

        private protected override void OnRender(CommandQueueDX11 cmd, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data)
        {
            if (_material == null)
                return;

            ApplyBuffers(cmd);
            ApplyResources(_material);
            _material.Object.Wvp.Value = Matrix4F.Multiply(data.RenderTransform, camera.ViewProjection);
            _material.Object.World.Value = data.RenderTransform;

            cmd.Draw(_material, _vertexCount, Topology);

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

        internal Material Material
        {
            get => _material;
            set => _material = value;
        }

        IMaterial IMesh.Material
        {
            get => _material;
            set => Material = value as Material;
        }

        public uint VertexCount => _vertexCount;

        public bool IsDynamic => _isDynamic;

        public float EmissivePower { get; set; } = 1.0f;
    }
}
