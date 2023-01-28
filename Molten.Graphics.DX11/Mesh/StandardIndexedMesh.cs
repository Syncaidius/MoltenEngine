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

        protected override void OnRender(GraphicsCommandQueue cmd, RenderService renderer, RenderCamera camera, ObjectRenderData data)
        {
            ApplyBuffers(cmd);
            IShaderResource normal = GetResource(1);

            if (Material == null)
            {
                RendererDX11 dx11Renderer = renderer as RendererDX11;

                // Use whichever default one fits the current configuration.
                if (normal == null)
                    Material = dx11Renderer.StandardMeshMaterial_NoNormalMap;
                else
                    Material = dx11Renderer.StandardMeshMaterial;

                Material.Object.EmissivePower.Value = EmissivePower;
            }

            Material.Object.World.Value = data.RenderTransform;
            Material.Object.Wvp.Value = Matrix4F.Multiply(data.RenderTransform, camera.ViewProjection);

            ApplyResources(Material);
            cmd.DrawIndexed(Material, _indexCount, Topology);
        }
    }
}
