namespace Molten.Graphics
{
    public class StandardMesh : Mesh<GBufferVertex>
    {
        internal StandardMesh(RendererDX11 renderer, uint maxVertices, VertexTopology topology, bool dynamic) : 
            base(renderer, maxVertices, topology, dynamic)
        {

        }

        private protected override void OnRender(CommandQueueDX11 cmd, RendererDX11 renderer, RenderCamera camera, ObjectRenderData data)
        {
            ApplyBuffers(cmd);
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
            cmd.Draw(mat, VertexCount, Topology);
        }
    }
}
