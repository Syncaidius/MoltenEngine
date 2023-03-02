namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="ComputeTask"/> headers.</summary>
    internal class ShaderVSParser : ShaderNodeParser<MaterialPass>
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Vertex;

        protected override void OnParse(MaterialPass pass, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            InitializeEntryPoint(pass, context, node, ShaderType.Vertex);
        }
    }
}
