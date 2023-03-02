namespace Molten.Graphics
{
    internal class ShaderPassParser : ShaderNodeParser<Material>
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Pass;

        protected override void OnParse(Material mat, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            MaterialPass pass = new MaterialPass(mat, "<Unnamed Material Pass>");

            for (int i = 0; i < mat.Samplers.Length; i++)
                pass.Samplers[i] = mat.Samplers[i];

            context.Compiler.ParseNode(pass, node.OriginalNode, context);
            mat.AddPass(pass);
        }
    }
}
