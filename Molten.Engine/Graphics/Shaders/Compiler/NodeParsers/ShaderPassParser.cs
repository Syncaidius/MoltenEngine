using System;

namespace Molten.Graphics
{
    internal class ShaderPassParser : ShaderNodeParser<HlslShader>
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Pass;

        protected override void OnParse(HlslShader shader, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            switch (shader)
            {
                case Material mat:
                    MaterialPass mPass = mat.Device.CreateMaterialPass(mat, "<Unnamed Material Pass>");

                    context.Compiler.ParseNode(mPass, node.OriginalNode, context);
                    mat.AddPass(mPass);
                    break;

                case ComputeTask task:
                    ComputePass cPass = task.Device.CreateComputePass(task, "<Unnamed Compute Pass>");

                    context.Compiler.ParseNode(cPass, node.OriginalNode, context);
                    task.AddPass(cPass);
                    break;

                default:
                    context.AddWarning($"Skipping <pass> tag: Unsupported HlslShader type of '{shader.GetType().Name}'");
                    break;
            }
        }
    }
}
