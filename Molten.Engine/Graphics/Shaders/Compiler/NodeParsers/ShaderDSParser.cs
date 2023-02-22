namespace Molten.Graphics
{
    /// <summary>An entry-point tag parser used by <see cref="ComputeTask"/> headers.</summary>
    internal class ShaderDSParser : ShaderNodeParser
    {
        public override ShaderNodeType NodeType => ShaderNodeType.Domain;

        public override Type[] TypeFilter { get; } = { typeof(Material), typeof(MaterialPass) };

        protected override void OnParse(HlslFoundation foundation, ShaderCompilerContext context, ShaderHeaderNode node)
        {
            if (node.Values.TryGetValue(ShaderHeaderValueType.Value, out string entryPoint))
            {
                switch (foundation)
                {
                    case Material material:
                        material.DefaultDSEntryPoint = entryPoint;
                        break;

                    case MaterialPass pass:
                        pass.DS.EntryPoint = entryPoint;
                        break;
                }
            }
            else
            {
                context.AddError("Domain shader <entry> tag is missing or empty.");
            }
        }
    }
}
