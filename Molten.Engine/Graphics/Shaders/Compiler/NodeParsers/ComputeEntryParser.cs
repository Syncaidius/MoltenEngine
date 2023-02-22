namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="ComputeTask"/> headers.</summary>
    internal class ComputeEntryParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Entry;

        public override Type[] TypeFilter { get; } = { typeof(ComputeTask) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string entryPoint))
                (foundation as ComputeTask).Composition.EntryPoint = entryPoint;
            else
                context.AddError("Compute task <entry> tag is missing or empty.");
        }
    }
}
