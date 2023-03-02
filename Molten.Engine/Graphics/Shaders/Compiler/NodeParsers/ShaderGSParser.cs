namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="Material"/> headers.</summary>
    internal class ShaderGSParser : ShaderNodeParser<MaterialPass>
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Geometry;

        protected override void OnParse(MaterialPass pass, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            InitializeEntryPoint(pass, context, node, ShaderType.Geometry);
        }
    }
}
