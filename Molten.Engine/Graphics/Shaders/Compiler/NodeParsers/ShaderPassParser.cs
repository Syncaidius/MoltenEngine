namespace Molten.Graphics
{
    internal class ShaderPassParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Pass;

        public override Type[] TypeFilter { get; } = { typeof(Material) };

        protected override void OnParse(HlslElement foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            Material mat = foundation as Material;
            MaterialPass pass = new MaterialPass(mat, "<Unnamed Material Pass>");

            for (int i = 0; i < mat.Samplers.Length; i++)
                pass.Samplers[i] = mat.Samplers[i];

            context.Compiler.ParseNode(pass, node.OriginalNode, context);
            mat.AddPass(pass);
        }
    }
}
