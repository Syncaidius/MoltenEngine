namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="ComputeTask"/> headers.</summary>
    internal class ShaderPSParser : ShaderNodeParser<MaterialPass>
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Pixel;

        protected override void OnParse(MaterialPass pass, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            InitializeEntryPoint(pass, context, node, ShaderType.Pixel);
        }
    }
}
