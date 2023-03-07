namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="HlslShader"/> headers.</summary>
    internal class ShaderVSParser : ShaderNodeParser<HlslPass>
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Vertex;

        protected override void OnParse(HlslPass pass, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            InitializeEntryPoint(pass, context, node, ShaderType.Vertex);
        }
    }
}
