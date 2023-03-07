using System;

namespace Molten.Graphics
{
    internal class ShaderPassParser : ShaderNodeParser<HlslShader>
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Pass;

        protected override void OnParse(HlslShader shader, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            HlslPass mPass = shader.Device.CreateShaderPass(shader, "<Unnamed Pass>");
            context.Compiler.ParseNode(mPass, node.OriginalNode, context);
            shader.AddPass(mPass);
        }
    }
}
