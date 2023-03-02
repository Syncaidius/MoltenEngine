namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="ComputeTask"/> headers.</summary>
    internal class ShaderDSParser : ShaderNodeParser<MaterialPass>
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Domain;

        protected override void OnParse(MaterialPass pass, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            InitializeEntryPoint(pass, context, node, ShaderType.Domain);
        }
    }
}
