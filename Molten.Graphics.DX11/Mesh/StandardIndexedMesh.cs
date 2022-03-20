namespace Molten.Graphics
{
    /// <summary>Represents an indexed mesh. These store mesh data by referring to vertices using index values stored in an index buffer. 
    /// In most cases this reduces the vertex data size drastically.</summary>
    /// <seealso cref="Molten.Graphics.IIndexedMesh" />
    public class StandardIndexedMesh : IndexedMesh<GBufferVertex>
    {
        internal StandardIndexedMesh(RendererDX11 renderer, uint maxVertices, uint maxIndices, VertexTopology topology, IndexBufferFormat indexFormat, bool dynamic) :
            base(renderer, maxVertices, maxIndices, topology, indexFormat, dynamic)
        { }

        private protected override void OnRender(DeviceContext pipe, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data)
        {
            ApplyBuffers(pipe);
            IShaderResource normal = GetResource(1);
            Material mat = _material;

            if (mat == null)
            {
                // Use whichever default one fits the current configuration.
                if (normal == null)
                    mat = renderer.StandardMeshMaterial_NoNormalMap;
                else
                    mat = renderer.StandardMeshMaterial;

                mat.Object.EmissivePower.Value = EmissivePower;
            }

            mat.Object.World.Value = data.RenderTransform;
            mat.Object.Wvp.Value = Matrix4F.Multiply(data.RenderTransform, camera.ViewProjection);

            ApplyResources(mat);
            renderer.Device.DrawIndexed(mat, _indexCount, Topology);
        }
    }
}
