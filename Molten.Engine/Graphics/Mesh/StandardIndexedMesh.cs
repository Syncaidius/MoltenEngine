namespace Molten.Graphics
{
    /// <summary>Represents an indexed mesh. These store mesh data by referring to vertices using index values stored in an index buffer. 
    /// In most cases this reduces the vertex data size drastically.</summary>
    public class StandardIndexedMesh : IndexedMesh<GBufferVertex>
    {
        internal StandardIndexedMesh(RenderService renderer, uint maxVertices, uint maxIndices, VertexTopology topology, IndexBufferFormat indexFormat, bool dynamic) :
            base(renderer, maxVertices, maxIndices, topology, indexFormat, dynamic)
        { }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            base.OnApply(cmd);

            IShaderResource normal = GetResource(1);
            if (Material == null)
            {
                // Use whichever default one fits the current configuration.
                if (normal == null)
                    Material = Renderer.StandardMeshMaterial_NoNormalMap;
                else
                    Material = Renderer.StandardMeshMaterial;

                Material.Object.EmissivePower.Value = EmissivePower;
            }
        }
    }
}
