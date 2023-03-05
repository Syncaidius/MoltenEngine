namespace Molten.Graphics
{
    internal class ShaderPassParser : ShaderNodeParser<Material>
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Pass;

        protected override void OnParse(Material mat, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            MaterialPass pass = mat.Device.CreateMaterialPass(mat, "<Unnamed Material Pass>");

            context.Compiler.ParseNode(pass, node.OriginalNode, context);
            mat.AddPass(pass);
        }
    }
}
