namespace Molten.Graphics
{
    public class StandardMesh : Mesh<GBufferVertex>
    {
        internal StandardMesh(RenderService renderer, BufferMode mode, uint maxVertices, IndexBufferFormat indexFormat, uint maxIndices, 
            GBufferVertex[] initialVertices = null, Array initialIndices = null) : 
            base(renderer, mode, maxVertices, indexFormat, maxIndices, initialVertices, initialIndices)
        {
        }

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
