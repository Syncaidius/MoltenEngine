namespace Molten.Graphics
{
    public class StandardMesh : Mesh<GBufferVertex>
    {
        internal StandardMesh(RenderService renderer, BufferMode mode, ushort maxVertices, uint maxIndices,
            GBufferVertex[] initialVertices = null, ushort[] initialIndices = null) :
            base(renderer, mode, maxVertices, maxIndices, initialVertices, initialIndices)
        { }

        internal StandardMesh(RenderService renderer, BufferMode mode, uint maxVertices, uint maxIndices,
            GBufferVertex[] initialVertices = null, uint[] initialIndices = null) :
            base(renderer, mode, maxVertices, maxIndices, initialVertices, initialIndices)
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
