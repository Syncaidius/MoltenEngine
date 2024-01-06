namespace Molten.Graphics;

internal class ShaderIterationParser : ShaderNodeParser
{
    public override ShaderNodeType NodeType => ShaderNodeType.Iterations;

    protected override void OnParse(ShaderDefinition def, ShaderPassDefinition passDef, ShaderCompilerContext context, ShaderHeaderNode node)
    {
        // Ignore if not part of a pass definition.
        if (passDef == null)
            return;

        if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string iterationValue))
        {
            if (int.TryParse(iterationValue, out int val))
            {
                passDef.Iterations = val;
            }
            else
            {
                context.AddWarning($"Invalid iteration number format for {passDef.GetType().Name}. Should be an integer value.");
                passDef.Iterations = 1;
            }
        }
        else
        {
            context.AddWarning($"Iteration value was not defined for <iterations> tag {passDef.GetType().Name}.");
        }
    }
}
