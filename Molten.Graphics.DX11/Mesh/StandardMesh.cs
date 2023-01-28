namespace Molten.Graphics
{
    public class StandardMesh : Mesh<GBufferVertex>
    {
        internal StandardMesh(RendererDX11 renderer, uint maxVertices, VertexTopology topology, bool dynamic) : 
            base(renderer, maxVertices, topology, dynamic)
        {

        }

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
            cmd.Draw(Material, VertexCount, Topology);
        }
    }
}
