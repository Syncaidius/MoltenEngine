namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="HlslShader"/> headers.</summary>
    internal class ShaderCSParser : ShaderNodeParser<HlslPass>
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Compute;

        protected override void OnParse(HlslPass pass, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if(InitializeEntryPoint(pass, context, node, ShaderType.Compute) != null)
                pass.Type = ShaderPassType.Compute;
        }
    }
}
