﻿namespace Molten.Graphics
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

            if (Material == null)
            {
                // Use whichever default one fits the current configuration.
                if (normal == null)
                    Material = renderer.StandardMeshMaterial_NoNormalMap;
                else
                    Material = renderer.StandardMeshMaterial;

                Material.Object.EmissivePower.Value = EmissivePower;
            }

            Material.Object.World.Value = data.RenderTransform;
            Material.Object.Wvp.Value = Matrix4F.Multiply(data.RenderTransform, camera.ViewProjection);

            ApplyResources(Material);
            cmd.Draw(Material, VertexCount, Topology);
        }
    }
}
