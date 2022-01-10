using Silk.NET.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public class Mesh<T> : Renderable, IMesh<T> where T : struct, IVertexType
    {
        // private protected is new in C# 7.2. See: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/private-protected
        private protected RendererDX11 _renderer;
        private protected BufferSegment _vb; 
        private protected Material _material;
        private protected int _vertexCount;
        private protected bool _isDynamic;

        internal Mesh(RendererDX11 renderer, int maxVertices, VertexTopology topology, bool dynamic) : base(renderer.Device)
        {
            _renderer = renderer;
            MaxVertices = maxVertices;
            Topology = topology;

            GraphicsBuffer vBuffer = dynamic ? renderer.DynamicVertexBuffer : renderer.StaticVertexBuffer;

            _vb = vBuffer.Allocate<T>((uint)MaxVertices);
            _vb.SetVertexFormat<T>();
        }

        public void SetVertices(T[] data)
        {
            SetVertices(data, 0, (int)data.Length);
        }

        public void SetVertices(T[] data, int count)
        {
            SetVertices(data, 0, count);
        }

        public void SetVertices(T[] data, int startIndex, int count)
        {
            _vertexCount = count;
            _vb.SetData(_renderer.Device, data, (uint)startIndex, (uint)count, 0, _renderer.StagingBuffer); // Staging buffer will be ignored if the mesh is dynamic.
        }

        internal virtual void ApplyBuffers(PipeDX11 pipe)
        {
            pipe.SetVertexSegment(_vb, 0);
        }

        private protected override void OnRender(PipeDX11 pipe, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data)
        {
            if (_material == null)
                return;

            ApplyBuffers(pipe);
            ApplyResources(_material);
            _material.Object.Wvp.Value = Matrix4F.Multiply(data.RenderTransform, camera.ViewProjection);
            _material.Object.World.Value = data.RenderTransform;

            renderer.Device.Draw(_material, (uint)_vertexCount, Topology);

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

        public int MaxVertices { get; }

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

        public int VertexCount => _vertexCount;

        public bool IsDynamic => _isDynamic;

        public float EmissivePower { get; set; } = 1.0f;
    }
}
