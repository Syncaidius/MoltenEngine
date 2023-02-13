namespace Molten.Graphics
{
    public class StandardMesh : Mesh<GBufferVertex>
    {
        internal StandardMesh(RenderService renderer, uint maxVertices, VertexTopology topology, bool dynamic) :
            base(renderer, maxVertices, topology, dynamic)
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
