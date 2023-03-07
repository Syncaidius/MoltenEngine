namespace Molten.Graphics
{
    public class StandardMesh : Mesh<GBufferVertex>
    {
        internal StandardMesh(RenderService renderer, uint maxVertices, PrimitiveTopology topology, bool dynamic) :
            base(renderer, maxVertices, topology, dynamic)
        { }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            base.OnApply(cmd);
            IShaderResource normal = GetResource(1);

            if (Shader == null)
            {
                // Use whichever default one fits the current configuration.
                if (normal == null)
                    Shader = Renderer.FxStandardMesh_NoNormalMap;
                else
                    Shader = Renderer.FxStandardMesh;

                Shader.Object.EmissivePower.Value = EmissivePower;
            }
        }
    }
}
