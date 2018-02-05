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
        protected RendererDX11 _renderer;
        BufferSegment _vb;
        int _maxVertices;
        PrimitiveTopology _topology;
        Material _material;

        IShaderValue _materialWvp;

        internal Mesh(RendererDX11 renderer, int maxVertices, VertexTopology topology) : base(renderer.Device)
        {
            _renderer = renderer;
            _maxVertices = maxVertices;
            _topology = topology.ToApi();

            _vb = renderer.StaticVertexBuffer.Allocate<T>(maxVertices);
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
            _vb.SetData(_renderer.Device.ExternalContext, data, startIndex, count, 0, _renderer.StagingBuffer);
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

            if (_materialWvp != null)
                _materialWvp.Value = Matrix.Multiply(data.RenderTransform, sceneData.ViewProjection);

            renderer.Device.Draw(_material, _vb.ElementCount, _topology);

            /* TODO: According to: https://www.gamedev.net/forums/topic/667328-vertices-and-indices-in-the-same-buffer/
            *  - A buffer can be bound as both a vertex and index buffer
            *  - If offsets and formats for each segment are correct, a single buffer can be bound at multiple pipeline stages.
            *  
            * TODO:
            *  - Remove Vertex and Index buffer
            *  - Let the renderer decide the bind flags of buffers
            *  - The above allows a combined vertex + index buffer.
            *  - Also opens the door for streamed buffers of any type instead of just vertex buffers
            *  - Simpler code maintainance, less buffer classes to deal with.
            *  - Check bind flags of buffer when setting as vertex and/or index buffer, in PipelineInput.
            */
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
                if(_material != value)
                {
                    _materialWvp = null;
                    _material = value ?? _renderer.DefaultMaterial;

                    if (_material != null)
                    {
                        if (_material.HasObjectConstants)
                            _materialWvp = _material["wvp"];
                    }
                }                
            }
        }

        IMaterial IMesh.Material
        {
            get => _material;
            set => Material = value as Material;
        }
    }
}
