namespace Molten.Graphics
{
    internal class ShaderIterationParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Iterations;

        public override Type[] TypeFilter => null;

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string iterationValue))
            {
                if (int.TryParse(iterationValue, out int val))
                {
                    foundation.Iterations = val;
                }
                else
                {
                    context.AddWarning($"Invalid iteration number format for {foundation.GetType().Name}. Should be an integer value.");
                    foundation.Iterations = 1;
                }
            }
            else
            {
                context.AddWarning($"Iteration value was not defined for <iterations> tag {foundation.GetType().Name}.");
            }
        }
    }
}
