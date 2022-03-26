using System.Xml;

namespace Molten.Graphics
{
    internal class ShaderIterationParser : FxcNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Iterations;

        public override Type[] TypeFilter => null;

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext<RendererDX11, HlslFoundation> context, ShaderHeaderNode node)
        {
            if (int.TryParse(node.Value, out int val))
            {
                foundation.Iterations = val;
            }
            else
            {
                context.AddWarning($"Invalid iteration number format for {foundation.GetType().Name}. Should be an integer value.");
                foundation.Iterations = 1;
            }
        }
    }
}
