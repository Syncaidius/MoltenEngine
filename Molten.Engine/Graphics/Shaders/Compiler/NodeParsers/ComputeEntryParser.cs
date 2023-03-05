namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="ComputeTask"/> headers.</summary>
    internal class ComputeEntryParser : ShaderNodeParser<ComputePass>
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Entry;

        protected override void OnParse(ComputePass pass, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string entryPoint))
                pass.Composition.EntryPoint = entryPoint;
            else
                context.AddError("Compute task <entry> tag is missing a value.");
        }
    }
}
