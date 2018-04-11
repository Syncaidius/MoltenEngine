using SharpDX.Direct3D;
using SharpDX.Direct3D11;
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
        private protected int _maxVertices;
        private protected PrimitiveTopology _topology;
        private protected Material _material;
        private protected int _vertexCount;
        private protected bool _isDynamic;

        private protected IShaderValue _matWvp;
        private protected IShaderValue _matWorld;

        internal Mesh(RendererDX11 renderer, int maxVertices, VertexTopology topology, bool dynamic) : base(renderer.Device)
        {
            _renderer = renderer;
            _maxVertices = maxVertices;
            _topology = topology.ToApi();

            SegmentedBuffer vBuffer = dynamic ? renderer.DynamicVertexBuffer : renderer.StaticVertexBuffer;

            _vb = vBuffer.Allocate<T>(maxVertices);
            _vb.SetVertexFormat<T>();
        }

        public void SetVertices(T[] data)
        {
            SetVertices(data, 0, data.Length);
        }

        public void SetVertices(T[] data, int count)
        {
            SetVertices(data, 0, count);
        }

        public void SetVertices(T[] data, int startIndex, int count)
        {
            _vertexCount = count;
            _vb.SetData(_renderer.Device.ExternalContext, data, startIndex, count, 0, _renderer.StagingBuffer); // Staging buffer will be ignored if the mesh is dynamic.
        }

        internal virtual void ApplyBuffers(GraphicsPipe pipe)
        {
            pipe.SetVertexSegment(_vb, 0);
        }

        internal override void Render(GraphicsPipe pipe, RendererDX11 renderer, ObjectRenderData data, SceneRenderDataDX11 sceneData)
        {
            if (_material == null)
                return;

            ApplyBuffers(pipe);
            ApplyResources(_material);

            if (_material.HasFlags(MaterialCommonFlags.Object))
            {
                _matWvp.Value = Matrix4F.Multiply(data.RenderTransform, sceneData.ViewProjection);
                _matWorld.Value = data.RenderTransform;
            }

            renderer.Device.Draw(_material, _vertexCount, _topology);

            /* TODO: According to: https://www.gamedev.net/forums/topic/667328-vertices-and-indices-in-the-same-buffer/
            *  - A buffer can be bound as both a vertex and index buffer
            *  - If offsets and formats for each segment are correct, a single buffer can be bound at multiple pipeline stages.
            */
        }

        protected virtual void OnSetMaterial(Material newMaterial)
        {
            // TODO enforce the below as requirements.
            // TODO set material to null if invalid. Scene will use default render shader if one was not provided (or set to null due to being invalid).
            if (newMaterial.HasFlags(MaterialCommonFlags.Object))
            {
                _matWvp = newMaterial["wvp"];
                _matWorld = newMaterial["world"];
            }
        }

        public virtual void Dispose()
        {
            IsVisible = false;
            _vb.Release();
        }

        public int MaxVertices => _maxVertices;

        public VertexTopology Topology => _topology.FromApi();

        internal Material Material
        {
            get => _material;
            set
            {
                if (_material != value)
                {
                    _matWvp = null;
                    _matWorld = null;

                    if (value != null)
                        OnSetMaterial(value);

                    _material = value;
                }    
            }
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
